using JS.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class SaveManager : ManualSingletonMono<SaveManager>
{
    private const int SAVE_SCHEMA_VERSION = 1;                 
    private static string GAME_VERSION => Application.version; 

    [Header("Security")]
    [SerializeField] private bool encryptSaves = true;

    private readonly Dictionary<string, ISaveable> registry = new();

    public static bool HasInstance => Instance != null;
    public static bool SkipLoad { get; set; } = false;

    private string saveFolder;

    private void Start()
    {
        if (SkipLoad)
        {
            SkipLoad = false;
            return;
        }
        Load(1);
    }

    private void Awake()
    {
        base.Awake();
        SaveSecret.Init();
        saveFolder = Path.Combine(Application.persistentDataPath, "SaveGame");
        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);
    }

    private string GetSavePath(int slotId, bool encryptedPreferred)
    {
        string scene = SceneManager.GetActiveScene().name;
        string name = encryptedPreferred
            ? $"{scene}_slot_{slotId}.savx"
            : $"{scene}_slot_{slotId}.sav";
        return Path.Combine(saveFolder, name);
    }

    private string GetMetaPath(bool encryptedPreferred)
    {
            string scene = SceneManager.GetActiveScene().name;
            return Path.Combine(saveFolder,
                encryptedPreferred ? $"{scene}_slots.meta" : $"{scene}_slots.json");
    }

    public void Registry(ISaveable saveble)
    {
        if (saveble == null) return;

        string id = saveble.GetUniqueId();
        if (string.IsNullOrWhiteSpace(id))
        {
            Debug.LogError($"[SaveManager] Registry FAILED — object {saveble} has null/empty ID!");
            return;
        }

        if (registry.TryGetValue(id, out var old) && old != null && old != saveble)
        {
            Debug.LogWarning($"[SaveManager] Duplicate ID detected: {id}, replaced old reference.");
            registry[id] = saveble; 
        }
        else
        {
            registry[id] = saveble; 
        }
    }

    public void UnRegistry(ISaveable saveble)
    {
        if (saveble == null) return;

        string id = saveble.GetUniqueId();
        if (string.IsNullOrWhiteSpace(id)) return;

        if (registry.Remove(id))
            Debug.Log($"[SaveManager] Unregistered {id}");
    }

    public async Task SaveAsync(int slotId = 1)
    {
        var stateDict = new Dictionary<string, string>();
        foreach (var kvp in registry)
        {
            object state = kvp.Value.CaptureState();
            string jsonState = JsonUtility.ToJson(state);
            stateDict[kvp.Key] = jsonState;
        }

        var wrapper = new SerializationWrapper(stateDict, SAVE_SCHEMA_VERSION, GAME_VERSION);
        wrapper.sceneName = SceneManager.GetActiveScene().name;
        string json = JsonUtility.ToJson(wrapper, true);

        string filePath = GetSavePath(slotId, encryptedPreferred: encryptSaves);
        string tempFile = filePath + ".tmp";

        await Task.Run(() =>
        {
            try
            {
                if (encryptSaves)
                {
                    byte[] key = SaveSecret.GetOrCreateKey();
                    byte[] payload = SaveCrypto.EncryptString(json, key);
                    File.WriteAllBytes(tempFile, payload);
                }
                else
                {
                    File.WriteAllText(tempFile, json, Encoding.UTF8);
                }

                File.Copy(tempFile, filePath, true);
                File.Delete(tempFile);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] SaveAsync failed: {ex}");
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        });

        SaveMeta(slotId);

        Debug.Log($"[SaveManager] SaveAsync done (slot {slotId})");
    }

    public void SaveSync(int slotId = 1)
    {
        var stateDict = new Dictionary<string, string>();
        foreach (var kvp in registry)
        {
            object state = kvp.Value.CaptureState();
            string jsonState = JsonUtility.ToJson(state);
            stateDict[kvp.Key] = jsonState;
        }

        var wrapper = new SerializationWrapper(stateDict, SAVE_SCHEMA_VERSION, GAME_VERSION);
        wrapper.sceneName = SceneManager.GetActiveScene().name;
        string json = JsonUtility.ToJson(wrapper, true);

        string filePath = GetSavePath(slotId, encryptedPreferred: encryptSaves);
        try
        {
            if (encryptSaves)
            {
                byte[] key = SaveSecret.GetOrCreateKey();
                byte[] payload = SaveCrypto.EncryptString(json, key);
                File.WriteAllBytes(filePath, payload);
            }
            else
            {
                File.WriteAllText(filePath, json, Encoding.UTF8);
            }
            SaveMeta(slotId);
            Debug.Log($"[SaveManager] SaveSync done (slot {slotId})");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveManager] SaveSync failed: {ex}");
        }
    }

    private void SaveMeta(int slotId)
    {
        string metaPath = GetMetaPath(encryptSaves);
        SerializationWrapper.SaveSlotMetaList list = new();

        try
        {
            if (File.Exists(metaPath))
            {
                if (encryptSaves && SaveCrypto.LooksEncrypted(File.ReadAllBytes(metaPath)))
                {
                    byte[] key = SaveSecret.GetOrCreateKey();
                    string metaJson = SaveCrypto.DecryptToString(File.ReadAllBytes(metaPath), key);
                    list = JsonUtility.FromJson<SerializationWrapper.SaveSlotMetaList>(metaJson);
                }
                else
                {
                    string metaJson = File.ReadAllText(metaPath, Encoding.UTF8);
                    list = JsonUtility.FromJson<SerializationWrapper.SaveSlotMetaList>(metaJson);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[SaveManager] Meta read failed, recreating. {ex.Message}");
            list = new SerializationWrapper.SaveSlotMetaList();
        }

        var existing = list.slots.Find(s => s.slotId == slotId);
        if (existing != null) list.slots.Remove(existing);

        list.slots.Add(new SerializationWrapper.SaveSlotMeta
        {
            slotId = slotId,
            saveTime = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            version = SAVE_SCHEMA_VERSION,
            gameVersion = GAME_VERSION
        });
        list.slots.Sort((a, b) => a.slotId.CompareTo(b.slotId));

        try
        {
            string metaJson = JsonUtility.ToJson(list, true);
            if (encryptSaves)
            {
                byte[] key = SaveSecret.GetOrCreateKey();
                byte[] payload = SaveCrypto.EncryptString(metaJson, key);
                File.WriteAllBytes(metaPath, payload);
            }
            else
            {
                File.WriteAllText(metaPath, metaJson, Encoding.UTF8);
            }

            string oldMeta = GetMetaPath(false);
            if (encryptSaves && File.Exists(oldMeta)) File.Delete(oldMeta);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveManager] Meta write failed: {ex}");
        }
    }

    public void Load(int slotId = 1)
    {
        string encPath = GetSavePath(slotId, true);   // .savx => encrypted
        string plainPath = GetSavePath(slotId, false); // .sav  => plaintext

        string json = null;

        try
        {
            if (File.Exists(encPath))
            {
                byte[] payload = File.ReadAllBytes(encPath);
                byte[] key = SaveSecret.GetOrCreateKey();
                json = SaveCrypto.DecryptToString(payload, key);
            }
            else if (File.Exists(plainPath))
            {
                byte[] bytes = File.ReadAllBytes(plainPath);
                string text = Encoding.UTF8.GetString(bytes);

                bool looksJson = !string.IsNullOrWhiteSpace(text) && text.TrimStart().StartsWith("{");
                if (looksJson)
                {
                    json = text;
                }
                else
                {
                    try
                    {
                        byte[] key = SaveSecret.GetOrCreateKey();
                        json = SaveCrypto.DecryptToString(bytes, key);
                        Debug.LogWarning("[SaveManager] Found encrypted payload in .sav (legacy). Migrating to .savx.");
                        File.WriteAllBytes(encPath, bytes);
                        File.Delete(plainPath);
                    }
                    catch (Exception decEx)
                    {
                        Debug.LogError($"[SaveManager] .sav is not JSON and cannot be decrypted. {decEx.Message}");
                        return;
                    }
                }
            }
            else
            {
                Debug.LogWarning($"[SaveManager] No save file found in slot {slotId}!");
                return;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveManager] Load failed (slot {slotId}): {ex}");
            return;
        }

        var wrapper = JsonUtility.FromJson<SerializationWrapper>(json);
        if (!string.IsNullOrEmpty(wrapper.sceneName) &&
            wrapper.sceneName != UnityEngine.SceneManagement.SceneManager.GetActiveScene().name)
            {
                Debug.Log($"[SaveManager]{wrapper.sceneName}");
                UnityEngine.SceneManagement.SceneManager.LoadScene(wrapper.sceneName);
                return;
            }

        if (wrapper == null)
        {
            Debug.LogError("[SaveManager] Parsed JSON is null. The file might be corrupted or not a valid SerializationWrapper.");
            return;
        }

        if (wrapper.version == 0)
        {
            Debug.LogWarning("[SaveManager] Legacy save (version 0). Applying migration -> 1.");
            MigrateIfNeeded(wrapper); 
        }
        else if (wrapper.version > SAVE_SCHEMA_VERSION)
        {
            Debug.LogWarning($"[SaveManager] Save file version ({wrapper.version}) is NEWER than runtime ({SAVE_SCHEMA_VERSION}). Attempting best-effort load.");
        }

        var stateDict = wrapper.ToDictionary();
        var snapshot = new List<ISaveable>(registry.Values);

        foreach (var obj in snapshot)
        {
            string id = obj.GetUniqueId();
            if (string.IsNullOrWhiteSpace(id)) continue;

            if (stateDict.TryGetValue(id, out string jsonState))
            {
                try
                {
                    Type saveType = obj.GetSaveType();
                    object state = JsonUtility.FromJson(jsonState, saveType);
                    obj.RestoreState(state);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[SaveManager] Failed to restore object '{id}': {ex.Message}");
                }
            }
        }
    }   

    // Pipeline migrate version
    private void MigrateIfNeeded(SerializationWrapper w)
    {
        switch (w.version)
        {
            case 0:
                // example change key's name: w.TryRenameKey("OldKey","NewKey");
                w.version = 1;
                w.gameVersion ??= GAME_VERSION;
                break;
                // case 1: // migrate to 2
                //     ...
                //     w.version = 2;
                //     break;
        }
    }

    public void ResetOnly(string id)
    {
        if (registry.TryGetValue(id, out ISaveable obj))
        {
            obj.ResetState();
        }
    }

    public void DeleteSave(int slotId)
    {
        string pathEnc = GetSavePath(slotId, true);
        string pathPlain = GetSavePath(slotId, false);

        if (File.Exists(pathEnc)) File.Delete(pathEnc);
        if (File.Exists(pathPlain)) File.Delete(pathPlain);

        string metaEnc = GetMetaPath(true);
        string metaPlain = GetMetaPath(false);

        SerializationWrapper.SaveSlotMetaList list = new();
        bool metaEncrypted = false;

        try
        {
            if (File.Exists(metaEnc))
            {
                metaEncrypted = true;
                byte[] key = SaveSecret.GetOrCreateKey();
                string metaJson = SaveCrypto.DecryptToString(File.ReadAllBytes(metaEnc), key);
                list = JsonUtility.FromJson<SerializationWrapper.SaveSlotMetaList>(metaJson);
            }
            else if (File.Exists(metaPlain))
            {
                string metaJson = File.ReadAllText(metaPlain, Encoding.UTF8);
                list = JsonUtility.FromJson<SerializationWrapper.SaveSlotMetaList>(metaJson);
            }

            var existing = list.slots.Find(s => s.slotId == slotId);
            if (existing != null) list.slots.Remove(existing);

            string newJson = JsonUtility.ToJson(list, true);
            if (metaEncrypted)
            {
                byte[] key = SaveSecret.GetOrCreateKey();
                byte[] payload = SaveCrypto.EncryptString(newJson, key);
                File.WriteAllBytes(metaEnc, payload);
            }
            else
            {
                File.WriteAllText(metaPlain, newJson, Encoding.UTF8);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveManager] DeleteSave meta update failed: {ex}");
        }
    }

    public List<SerializationWrapper.SaveSlotMeta> GetAllSlotMeta()
    {
        string metaEnc = GetMetaPath(true);
        string metaPlain = GetMetaPath(false);

        try
        {
            if (File.Exists(metaEnc))
            {
                byte[] key = SaveSecret.GetOrCreateKey();
                string metaJson = SaveCrypto.DecryptToString(File.ReadAllBytes(metaEnc), key);
                var list = JsonUtility.FromJson<SerializationWrapper.SaveSlotMetaList>(metaJson);
                return list.slots;
            }
            if (File.Exists(metaPlain))
            {
                string metaJson = File.ReadAllText(metaPlain, Encoding.UTF8);
                var list = JsonUtility.FromJson<SerializationWrapper.SaveSlotMetaList>(metaJson);
                return list.slots;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveManager] GetAllSlotMeta failed: {ex}");
        }

        return new List<SerializationWrapper.SaveSlotMeta>();
    }

    private void OnApplicationQuit()
    {
        try
        {
            SaveSync(1); 
            Debug.Log("[SaveManager] Auto-saved on quit.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SaveManager] Auto-save on quit failed: {ex}");
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            try
            {
                SaveSync(1); 
                Debug.Log("[SaveManager] Auto-saved on pause.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Auto-save on pause failed: {ex}");
            }
        }
    }
}

[System.Serializable]
public class SerializationWrapper
{
    public int version;      
    public string gameVersion;
    public string sceneName;

    [System.Serializable]
    public class Entry { public string key; public string value; }

    public List<Entry> entries = new();

    public SerializationWrapper() { }

    public SerializationWrapper(Dictionary<string, string> dict, int schemaVersion = 1, string gameVer = null)
    {
        version = schemaVersion;
        gameVersion = gameVer;
        foreach (var kv in dict)
            entries.Add(new Entry { key = kv.Key, value = kv.Value });
    }

    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        foreach (var e in entries) dict[e.key] = e.value;
        return dict;
    }

    public bool TryGet(string key, out string value)
    {
        foreach (var e in entries)
        {
            if (e.key == key) { value = e.value; return true; }
        }
        value = null;
        return false;
    }

    public bool TryRenameKey(string oldKey, string newKey)
    {
        for (int i = 0; i < entries.Count; i++)
        {
            if (entries[i].key == oldKey)
            {
                entries[i].key = newKey;
                return true;
            }
        }
        return false;
    }

    [System.Serializable]
    public class SaveSlotMeta
    {
        public int slotId;
        public string saveTime;
        public int version;          
        public string gameVersion;   
    }

    [System.Serializable]
    public class SaveSlotMetaList { public List<SaveSlotMeta> slots = new(); }
}