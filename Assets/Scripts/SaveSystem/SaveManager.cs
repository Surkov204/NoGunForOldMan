using JS.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class SaveManager : ManualSingletonMono<SaveManager>
{
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
        saveFolder = Path.Combine(Application.persistentDataPath, "SaveGame");
        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);
    }

    private string GetSavePath(int slotId, bool encryptedPreferred)
    {
        string name = encryptedPreferred ? $"save_slot_{slotId}.savx" : $"save_slot_{slotId}.sav";
        return Path.Combine(saveFolder, $"save_slot_{slotId}.sav");
    }

    private string GetMetaPath(bool encryptedPreferred)
    {
        return Path.Combine(saveFolder, encryptedPreferred ? "slots.meta" : "slots.json");
    }

    public void Registry(ISaveable saveble)
    {
        string id = saveble.GetUniqueId();
        if (!registry.ContainsKey(id))
            registry[id] = saveble;
    }

    public void UnRegistry(ISaveable saveble)
    {
        string id = saveble.GetUniqueId();
        if (registry.ContainsKey(id))
            registry.Remove(id);
    }

    public void Save(int slotId = 1)
    {
        var stateDict = new Dictionary<string, string>();

        foreach (var kvp in registry)
        {
            object state = kvp.Value.CaptureState();
            string jsonState = JsonUtility.ToJson(state);
            stateDict[kvp.Key] = jsonState;
        }

        var wrapper = new SerializationWrapper(stateDict);
        string json = JsonUtility.ToJson(wrapper, true);

        string filePath = GetSavePath(slotId, encryptedPreferred: encryptSaves);

        string tempFile = filePath + ".tmp";

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
            Debug.LogError($"[SaveManager] Save failed: {ex}");
            if (File.Exists(tempFile)) File.Delete(tempFile);
            return;
        }

        // ---- Update meta ----
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
            saveTime = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
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
        ;
    }

    public void Load(int slotId = 1)
    {
        // Ưu tiên đọc .savx (encrypted). Nếu không có, rơi về .sav (plaintext, backward-compat)
        string encPath = GetSavePath(slotId, encryptedPreferred: true);
        string plainPath = GetSavePath(slotId, encryptedPreferred: false);

        string json = null;

        try
        {
            if (File.Exists(encPath))
            {
                byte[] payload = File.ReadAllBytes(encPath);

                if (SaveCrypto.LooksEncrypted(payload))
                {
                    byte[] key = SaveSecret.GetOrCreateKey();
                    json = SaveCrypto.DecryptToString(payload, key);
                }
                else
                {
                    json = File.ReadAllText(encPath, Encoding.UTF8);
                    Debug.LogWarning($"[SaveManager] Slot {slotId} file không mã hoá, load như JSON thường.");
                }
            }
            else if (File.Exists(plainPath))
            {
                json = File.ReadAllText(plainPath, Encoding.UTF8);
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
        var stateDict = wrapper.ToDictionary();

        foreach (var kvp in registry)
        {
            if (stateDict.TryGetValue(kvp.Key, out string jsonState))
            {
                object dummy = kvp.Value.CaptureState();
                object state = JsonUtility.FromJson(jsonState, dummy.GetType());
                kvp.Value.RestoreState(state);
            }
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
        string pathEnc = GetSavePath(slotId, encryptedPreferred: true);
        string pathPlain = GetSavePath(slotId, encryptedPreferred: false);

        if (File.Exists(pathEnc)) File.Delete(pathEnc);
        if (File.Exists(pathPlain)) File.Delete(pathPlain);

        // update meta (encrypted hoặc plaintext – đọc linh hoạt)
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
}

[System.Serializable]
public class SerializationWrapper
{
    [System.Serializable]
    public class Entry { public string key; public string value; }

    public List<Entry> entries = new();

    public SerializationWrapper() { }

    public SerializationWrapper(Dictionary<string, string> dict)
    {
        foreach (var kv in dict)
            entries.Add(new Entry { key = kv.Key, value = kv.Value });
    }

    public Dictionary<string, string> ToDictionary()
    {
        var dict = new Dictionary<string, string>();
        foreach (var e in entries) dict[e.key] = e.value;
        return dict;
    }

    [System.Serializable]
    public class SaveSlotMeta { public int slotId; public string saveTime; }

    [System.Serializable]
    public class SaveSlotMetaList { public List<SaveSlotMeta> slots = new(); }
}