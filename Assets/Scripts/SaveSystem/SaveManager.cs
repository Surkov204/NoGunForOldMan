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

    private void Start()
    {
        if (SkipLoad)
        {
            Debug.Log("[SaveManager] Skipping load this time");
            SkipLoad = false; 
            return;
        }
        Load();
    }

    private void Awake()
    {
        base.Awake();
        string customFolder = Path.Combine("E:/SaveGame");
        saveFile = Path.Combine(customFolder,"savegame.sav");
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

    public void Save()
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

        string dir = Path.GetDirectoryName(saveFile);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        File.WriteAllText(saveFile, json);
        Debug.Log($"[SaveManager] Game saved to {saveFile}");
    }

    public void Load()
    {
        if (!File.Exists(saveFile))
        {
            Debug.LogWarning("[SaveManager] No save file found!");
            return;
        }

        string json = File.ReadAllText(saveFile);
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

        Debug.Log($"[SaveManager] Game loaded from {saveFile}");
    }

    public void ResetOnly(string id)
    {
        if (registry.TryGetValue(id, out ISaveable obj))
        {
            obj.ResetState();
        }
    }

    public void DeleteSave()
    {
        if (File.Exists(saveFile))
        {
            File.Delete(saveFile);
            Debug.Log($"[SaveManager] Save file deleted at {saveFile}");
        }
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
}