using JS;
using JS.Utils;
using UnityEngine;

public class UIController : MonoBehaviour
{
    private bool isPauseMenuShowing;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePauseMenu();
    }

    public void TogglePauseMenu()
    {

        if (UiManager.Instance.IsUIShowing(UIName.GameSettingScreen))
        {
            UiManager.Instance.HideUI(UIName.GameSettingScreen);
            return;
        }

        if (!isPauseMenuShowing) ShowPauseMenu();
        else HidePauseMenu();
    }

    public void ShowPauseMenu()
    {
        UiManager.Instance.ShowUI(UIName.PauseGameScreen);
    }

    public void HidePauseMenu()
    {
        UiManager.Instance.HideUI(UIName.PauseGameScreen);
        isPauseMenuShowing = false;
        Time.timeScale = 1;
    }

}
