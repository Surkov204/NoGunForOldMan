using JS;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UiGameOverScreen : UIBase
{
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
