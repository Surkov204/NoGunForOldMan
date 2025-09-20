using JS;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class UIPauseMenuScreen : UIBase
{
    [SerializeField] private Button continueButton;
    [SerializeField] private Button settingButton;
    [SerializeField] private Button replayButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private string gameplaySceneName = "";
    [SerializeField] private string SelectLevelSceneName = "SelectMap";

    protected override void Awake()
    {
        base.Awake();

        continueButton.onClick.AddListener(OnContinueClick);
        settingButton.onClick.AddListener(OnSettingClick);
        replayButton.onClick.AddListener(OnReplayClick);
        quitButton.onClick.AddListener(OnQuitClick);
    }

    public void OnContinueClick()
    {
        UIManager.Instance.Hide(UIName.PauseGameScreen);
        Time.timeScale = 1f;
    }

    public void OnSettingClick()
    {
        UIManager.Instance.Show(UIName.GameSettingScreen);
    }

    public void OnReplayClick()
    {
        UIManager.Instance.Back();
        SaveManager.SkipLoad = true;
        SaveManager.Instance.ResetOnly("Player");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1f;
    }

    public void OnQuitClick()
    {
        SceneLoader.Load(gameplaySceneName);
        SaveManager.Instance.Save();
        Time.timeScale = 1f;
    }

}
