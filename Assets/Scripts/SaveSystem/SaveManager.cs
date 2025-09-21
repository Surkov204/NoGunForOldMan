using JS.Utils;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : ManualSingletonMono<SaveManager>
{
    private readonly Dictionary<string, ISaveable> registry = new();
    public static bool HasInstance => Instance != null;
    public static bool SkipLoad { get; set; } = false;
    private string saveFile;
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

    private string GetSavePath(int slotId)
    {
        return Path.Combine(saveFolder, $"save_slot_{slotId}.sav");
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

        string filePath = GetSavePath(slotId);

        string tempFile = filePath + ".tmp";
        File.WriteAllText(tempFile, json);
        File.Copy(tempFile, filePath, true);
        File.Delete(tempFile);

        string metaFile = Path.Combine(saveFolder, "slots.json");
        SerializationWrapper.SaveSlotMetaList list;

        if (File.Exists(metaFile)) {
            list = JsonUtility.FromJson<SerializationWrapper.SaveSlotMetaList>(File.ReadAllText(metaFile));
        }
        else
        {
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
        File.WriteAllText(metaFile, JsonUtility.ToJson(list, true));
    }

    public void Load(int slotId = 1)
    {
        string filePath = GetSavePath(slotId);
        if (!File.Exists(filePath))
        {
            Debug.LogWarning($"[SaveManager] No save file found in slot {slotId}!");
            return;
        }

        string json = File.ReadAllText(filePath);
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
        string filePath = GetSavePath(slotId);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        string metaFile = Path.Combine(saveFolder, "slots.json");
        if (File.Exists(metaFile))
        {
            var list = JsonUtility.FromJson<SerializationWrapper.SaveSlotMetaList>(File.ReadAllText(metaFile));
            var existing = list.slots.Find(s => s.slotId == slotId);
            if (existing != null) list.slots.Remove(existing);
            File.WriteAllText(metaFile, JsonUtility.ToJson(list, true));
        }
    }

    public List<SerializationWrapper.SaveSlotMeta> GetAllSlotMeta()
    {
        string metaFile = Path.Combine(saveFolder, "slots.json");
        if (!File.Exists(metaFile)) return new List<SerializationWrapper.SaveSlotMeta>();

        string json = File.ReadAllText(metaFile);
        var list = JsonUtility.FromJson<SerializationWrapper.SaveSlotMetaList>(json);
        return list.slots;
    }
}

[System.Serializable]
public class SerializationWrapper
{
    [System.Serializable]
    public class Entry
    {
        public string key;
        public string value;
    }

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
    public class SaveSlotMeta {
        public int slotId;
        public string saveTime;
    }

    [System.Serializable]
    public class SaveSlotMetaList {
        public List<SaveSlotMeta> slots = new();
    }
}