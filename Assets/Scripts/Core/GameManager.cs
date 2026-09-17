using UnityEngine;

namespace CrashMissileCrash.Core
{
    public enum GameState
    {
        Boot,
        Home,
        Tutorial,
        LevelLoading,
        Battle,
        Victory,
        Defeat,
        Paused
    }

    public readonly struct GameStateChangedEvent : IGameEvent
    {
        public readonly GameState Previous;
        public readonly GameState Current;
        public GameStateChangedEvent(GameState previous, GameState current)
        {
            Previous = previous;
            Current = current;
        }
    }

    /// <summary>
    /// Top-level state machine for the application. Does not know the details of any
    /// subsystem (battle, UI, economy) - it only orchestrates high level state and
    /// broadcasts transitions over the EventBus so each system reacts independently.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.Boot;
        public GameState PreviousState { get; private set; } = GameState.Boot;

        private GameState _stateBeforePause;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            ServiceLocator.Register(this);
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
        }

        public void ChangeState(GameState newState)
        {
            if (newState == CurrentState) return;
            PreviousState = CurrentState;
            CurrentState = newState;
            EventBus.Publish(new GameStateChangedEvent(PreviousState, CurrentState));
        }

        public void PauseGame()
        {
            if (CurrentState == GameState.Paused) return;
            _stateBeforePause = CurrentState;
            Time.timeScale = 0f;
            ChangeState(GameState.Paused);
        }

        public void ResumeGame()
        {
            if (CurrentState != GameState.Paused) return;
            Time.timeScale = 1f;
            ChangeState(_stateBeforePause);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && CurrentState == GameState.Battle)
            {
                PauseGame();
            }
        }
    }
}
