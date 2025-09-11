using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (GameStateManager.Instance.CurrentState == GameState.Playing)
                GameStateManager.Instance.ChangeState(GameState.Paused);
            else if (GameStateManager.Instance.CurrentState == GameState.Paused)
                GameStateManager.Instance.ChangeState(GameState.Playing);
        }
    }
}
