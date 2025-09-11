using UnityEngine;
using JS;
using UnityEngine.UI;
public class UIGameSetting : BaseUI
{
    [SerializeField] private Button quitButton;
    private void Awake()
    {
        base.Awake();
        quitButton.onClick.AddListener(quitSettingPopup);
        
    }
    private void quitSettingPopup() {
        GameStateManager.Instance.ChangeState(GameState.Paused);
    }
}
