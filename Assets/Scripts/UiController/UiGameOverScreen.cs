using JS;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UiGameOverScreen : BaseUI
{
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
