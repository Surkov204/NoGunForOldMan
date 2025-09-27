using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class SaveSlotUI : MonoBehaviour
{
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private TextMeshProUGUI textName;
    [SerializeField] private TextMeshProUGUI timeText;

    private int slotId;

    public void Init(int id, string saveTime)
    {
        slotId = id;
        textName.text = $"Slot {slotId}";
        timeText.text = saveTime;
        saveButton.onClick.AddListener(() => { 
            SaveManager.Instance.Save(slotId);
            if (UIManager.Instance.IsVisible(JS.UIName.GameSave)) {
                UIManager.Instance.Show(JS.UIName.SaveBoard);
            }
            DOVirtual.DelayedCall(2f, () =>
            {
                UIManager.Instance.Hide(JS.UIName.SaveBoard);
            });
        }); 
        loadButton.onClick.AddListener(() =>
        {
            SaveManager.Instance.Load(slotId);
            UIManager.Instance.HideAll();
        });
    }
}
