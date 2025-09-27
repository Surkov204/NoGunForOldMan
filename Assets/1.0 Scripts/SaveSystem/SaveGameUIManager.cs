using JS.Utils;
using System.Collections.Generic;
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
        deleteAllButton.onClick.AddListener(DeleteAllSlots);
        LoadSlotMetadata();
    }

    private void LoadSlotMetadata()
    {
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        var slots = SaveManager.Instance.GetAllSlotMeta();
        int maxId = 0;

        foreach (var meta in slots)
        {
            AddSlotUI(meta.slotId, meta.saveTime);
            if (meta.slotId > maxId) maxId = meta.slotId;
        }
        currentSlotCount = maxId;
    }

    private void AddNewSlot()
    {
        currentSlotCount++;
        SaveManager.Instance.Save(currentSlotCount);
        AddSlotUI(currentSlotCount, System.DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
    }

    private void AddSlotUI(int slotId, string saveTime)
    {
        GameObject slotObj = Instantiate(saveSlotPrefab, contentParent);
        SaveSlotUI slotUI = slotObj.GetComponent<SaveSlotUI>();
        slotUI.Init(slotId, saveTime);
    }

    private void DeleteAllSlots()
    {
        var slots = SaveManager.Instance.GetAllSlotMeta();
        foreach (var meta in slots)
        {
            SaveManager.Instance.DeleteSave(meta.slotId);
        }

        currentSlotCount = 0;

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        Debug.Log("All save slots deleted!");
    }
}
