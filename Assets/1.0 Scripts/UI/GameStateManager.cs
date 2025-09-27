using JS;
using JS.Utils;
using UnityEngine;
using System.Collections.Generic;

public class GameStateManager : ManualSingletonMono<GameStateManager>
{
    public GameState CurrentState { get; private set; } = GameState.Playing;
    public GameState? CurrentOverlay { get; private set; } = null;

    private static readonly Dictionary<GameState, UIName> stateToUI = new()
    {
        { GameState.Paused, UIName.PauseGameScreen },
        { GameState.GameSetting, UIName.GameSettingScreen },
    };

    private void Awake()
    {
        base.Awake();
    }

    public void ChangeState(GameState newState)
    {
        if (!stateToUI.TryGetValue(newState, out var uiName))
        {
            OnExitState(CurrentState);
            CurrentState = newState;
            HandleStateChange(newState);
            return;
        }

        var baseUI = UiManager.Instance.GetUI(uiName);
        if (baseUI == null)
        {
            UiManager.Instance.ShowUI(uiName);
            baseUI = UiManager.Instance.GetUI(uiName);
        }
        if (baseUI == null) return;

        if (baseUI.PopupType == PopupType.Normal)
        {
            if (CurrentOverlay.HasValue)
            {
                OnExitState(CurrentOverlay.Value);
                CurrentOverlay = null;
            }

            if (newState == CurrentState) return;

            if (CurrentState != newState)
            {
                OnExitState(CurrentState);
                CurrentState = newState;
                OnEnterState(CurrentState);
            }
            else
            {
                OnExitState(CurrentState);
                CurrentState = GameState.None;
            }
        }
        else if (baseUI.PopupType == PopupType.Overlay)
        {
            if (CurrentOverlay == newState) return;

            if (CurrentOverlay.HasValue)
                OnExitState(CurrentOverlay.Value);

            CurrentOverlay = newState;
            OnEnterState(CurrentOverlay.Value);
        }
    }

    public void CloseOverlay()
    {
        if (CurrentOverlay.HasValue)
        {
            OnExitState(CurrentOverlay.Value);
            CurrentOverlay = null;
        }
    }

    private void HandleStateChange(GameState state)
    {
        switch (state)
        {
            case GameState.Playing:
                Time.timeScale = 1f;
                break;
            case GameState.Paused:
                Time.timeScale = 0f;
                break;
        }
    }

    private void OnExitState(GameState state)
    {
        if (stateToUI.TryGetValue(state, out var ui))
            UiManager.Instance.HideUI(ui);
    }

    private void OnEnterState(GameState state)
    {
        if (stateToUI.TryGetValue(state, out var ui))
            UiManager.Instance.ShowUI(ui);

        if (state == GameState.Playing)
            Time.timeScale = 1f;
        if (state == GameState.Paused)
            Time.timeScale = 0f;
        if (state == GameState.GameSetting)
            Time.timeScale = 0f;
    }
}
