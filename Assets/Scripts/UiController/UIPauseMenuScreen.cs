using JS;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class UIPauseMenuScreen : BaseUI
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
        GameStateManager.Instance.ChangeState(GameState.Playing);
    }

    public void OnSettingClick()
    {
        GameStateManager.Instance.ChangeState(GameState.GameSetting);
    }

    public void OnReplayClick()
    {
        UiManager.Instance.HideAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1;
    }

    public void OnQuitClick()
    {
        SceneLoader.Load(gameplaySceneName);
        Time.timeScale = 1f;
    }

}
