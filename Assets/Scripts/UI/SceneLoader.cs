using UnityEngine.SceneManagement;

public static class SceneLoader
{
    private static string targetSceneName;

    public static void Load(string sceneName)
    {
        targetSceneName = sceneName;
        SceneManager.LoadScene("Loading");
    }

    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetSceneName);
    }
}
