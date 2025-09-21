using JS.Utils;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine;
using static SerializationWrapper;
using JS;

public class SaveGameUIManager : UIBase
{
    [SerializeField] private Transform contentParent;   
    [SerializeField] private GameObject saveSlotPrefab; 
    [SerializeField] private Button addSlotButton;
    [SerializeField] private Button deleteAllButton;

    private int currentSlotCount = 0;

    private void Start()
    {
        addSlotButton.onClick.AddListener(AddNewSlot);
        deleteAllButton.onClick.AddListener(DeleteAllSlots); ;
        LoadSlotMetadata();
    }

    private void LoadSlotMetadata()
    {
        string metaFile = Path.Combine(Application.persistentDataPath, "SaveGame", "slots.json");
        if (!File.Exists(metaFile)) return;

        string json = File.ReadAllText(metaFile);
        var wrapper = JsonUtility.FromJson<SaveSlotMetaList>(json);

        foreach (var meta in wrapper.slots)
        {
            AddSlotUI(meta.slotId, meta.saveTime);
        }
    }

    private void AddNewSlot()
    {
        currentSlotCount++;
        SaveManager.Instance.Save(currentSlotCount);
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }
        LoadSlotMetadata();
    }

    private void AddSlotUI(int slotId, string saveTime)
    {
        GameObject slotObj = Instantiate(saveSlotPrefab, contentParent);
        SaveSlotUI slotUI = slotObj.GetComponent<SaveSlotUI>();
        slotUI.Init(slotId, saveTime);
    }

    private void DeleteAllSlots()
    {
        string saveFolder = Path.Combine(Application.persistentDataPath, "SaveGame");
        if (Directory.Exists(saveFolder))
        {
            Directory.Delete(saveFolder, true);
        }

        currentSlotCount = 0;

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        Debug.Log("All save slots deleted!");
    }
}
