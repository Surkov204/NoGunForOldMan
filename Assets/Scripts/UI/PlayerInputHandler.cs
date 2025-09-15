using JS;
using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (UIManager.Instance.HasPopupOnTop)
            {
                UIManager.Instance.Back();
            }
            else
            {
                var pauseUI = UIName.PauseGameScreen;
                if (UIManager.Instance.IsVisible(pauseUI))
                {
                    UIManager.Instance.Hide(pauseUI);
                }
                else
                {
                    UIManager.Instance.Show(pauseUI);
                }
            }
        }
    }
}
