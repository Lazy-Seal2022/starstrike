using UnityEngine;
using UnityEngine.SceneManagement;
using StarStrike.Core;
using StarStrike.Platform.Storage;

namespace StarStrike.Gameplay
{
    public enum GameState
    {
        Boot,
        Menu,
        Playing,
        Paused,
        GameOver
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.Boot;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Auto-upgrade Player GameObject from legacy ShipController to Domain-Driven components
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.gravityScale = 0f;
                    rb.linearDamping = 0.5f;
                    rb.angularDamping = 1.0f;
                }

                var sr = player.GetComponent<SpriteRenderer>();
                if (sr == null) sr = player.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 10;

                var legacy = player.GetComponent("ShipController");
                if (legacy != null)
                {
                    DestroyImmediate(legacy); // Remove the old script
                    
                    // Add new Domain-Driven components
                    var pp = player.AddComponent<PlayerProgression>();
                    pp.health = player.AddComponent<ShipHealth>();
                    pp.movement = player.AddComponent<ShipMovement>();
                    pp.weapon = player.AddComponent<ShipWeapon>();
                    pp.shipRenderer = sr;
                    
                    player.AddComponent<PlayerInputReader>();
                }
            }
        }

        private void Start()
        {
            ChangeState(GameState.Menu);
        }

        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;
            
            GameState oldState = CurrentState;
            CurrentState = newState;

            OnStateChanged(oldState, newState);
        }

        private void OnStateChanged(GameState oldState, GameState newState)
        {
            switch (newState)
            {
                case GameState.Menu:
                    Time.timeScale = 1f;
                    // Logic to show menu UI (using GameEvents)
                    break;
                case GameState.Playing:
                    Time.timeScale = 1f;
                    // Ensure gameplay is active
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    Time.timeScale = 1f;
                    break;
            }
        }

        public void StartGame()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = Vector3.zero;
                var rb = player.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = Vector2.zero;
                    rb.angularVelocity = 0f;
                    rb.gravityScale = 0f;
                }
            }
            if (Camera.main != null)
            {
                Camera.main.transform.position = new Vector3(0, 0, -10);
            }

            if (PlayerProgression.Instance != null)
            {
                PlayerProgression.Instance.ResetForNewRun();
            }
            WorldManager wm = FindObjectOfType<WorldManager>();
            if (wm != null)
            {
                wm.ResetWorld();
            }
            ChangeState(GameState.Playing);
        }

        public void PauseGame()
        {
            if (CurrentState == GameState.Playing)
            {
                ChangeState(GameState.Paused);
            }
        }

        public void ResumeGame()
        {
            if (CurrentState == GameState.Paused)
            {
                ChangeState(GameState.Playing);
            }
        }

        public void GameOver()
        {
            ChangeState(GameState.GameOver);
        }

        public void RestartGame()
        {
            ChangeState(GameState.Boot);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
