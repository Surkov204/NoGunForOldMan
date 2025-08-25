using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button howToPlayButton;
    [SerializeField] private Button quitButton;

    [Header("Scene Settings")]
    [SerializeField] private string gameplaySceneName = "NoGunForOldMan";

    private void Awake()
    {
        if (playButton) playButton.onClick.AddListener(OnPlayClicked);
        if (howToPlayButton) howToPlayButton.onClick.AddListener(OnHowToPlayClicked);
        if (quitButton) quitButton.onClick.AddListener(OnQuitClicked);
    }

    private void OnPlayClicked()
    {
        SceneLoader.Load(gameplaySceneName);
    }

    private void OnHowToPlayClicked()
    {
        
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
