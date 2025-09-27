using UnityEngine;
using JS;
using UnityEngine.UI;
public class UIGameSetting : UIBase
{
    [SerializeField] private Button quitButton;
    private void Awake()
    {
        base.Awake();
        quitButton.onClick.AddListener(quitSettingPopup);
        
    }
    private void quitSettingPopup() {
        UIManager.Instance.Hide(UIName.GameSettingScreen);
    }
}
