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
        GameOver,
        Victory
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; } = GameState.Boot;

        [Header("Mode")]
        public GameMode CurrentMode { get; private set; } = GameMode.PvE;

        public void SetGameMode(GameMode mode)
        {
            CurrentMode = mode;
        }

        [Header("Run Metrics")]
        public float RunTimer { get; private set; }
        public int GemsCollectedInRun { get; private set; }
        public int AsteroidsMinedCount { get; private set; }
        public int EnemiesDestroyedCount { get; private set; }
        public RunSummary LastRunSummary { get; private set; }

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
                    DestroyImmediate(legacy);
                    
                    var pp = player.AddComponent<PlayerProgression>();
                    pp.health = player.AddComponent<ShipHealth>();
                    pp.movement = player.AddComponent<ShipMovement>();
                    pp.weapon = player.AddComponent<ShipWeapon>();
                    pp.shipRenderer = sr;
                    
                    player.AddComponent<PlayerInputReader>();
                }
            }
        }

        private void OnEnable()
        {
            GameEvents.OnAsteroidDestroyed += HandleAsteroidDestroyed;
            GameEvents.OnGemCollected += HandleGemCollected;
            GameEvents.OnEnemyDestroyed += HandleEnemyDestroyed;
        }

        private void OnDisable()
        {
            GameEvents.OnAsteroidDestroyed -= HandleAsteroidDestroyed;
            GameEvents.OnGemCollected -= HandleGemCollected;
            GameEvents.OnEnemyDestroyed -= HandleEnemyDestroyed;
        }

        private void Start()
        {
            ChangeState(GameState.Menu);
        }

        private void Update()
        {
            if (CurrentState == GameState.Playing)
            {
                RunTimer += Time.deltaTime;
            }
        }

        private void HandleAsteroidDestroyed(Vector2 pos, string size)
        {
            if (CurrentState == GameState.Playing)
            {
                AsteroidsMinedCount++;
            }
        }

        private void HandleGemCollected(int added, int total)
        {
            if (CurrentState == GameState.Playing && added > 0)
            {
                GemsCollectedInRun += added;
            }
        }

        private void HandleEnemyDestroyed(string enemyId)
        {
            if (CurrentState == GameState.Playing)
            {
                EnemiesDestroyedCount++;
            }
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
                    break;
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                case GameState.Victory:
                    Time.timeScale = 1f;
                    break;
            }
        }

        public void StartGame()
        {
            RunTimer = 0f;
            GemsCollectedInRun = 0;
            AsteroidsMinedCount = 0;
            EnemiesDestroyedCount = 0;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                var pp = Resources.FindObjectsOfTypeAll<PlayerProgression>();
                if (pp.Length > 0)
                {
                    player = pp[0].gameObject;
                    player.SetActive(true);
                }
            }
            if (player != null)
            {
                player.SetActive(true);
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
            GameEvents.TriggerGameStarted();
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
            if (CurrentState != GameState.Playing) return;

            string shipId = PlayerProgression.Instance != null && PlayerProgression.Instance.currentShip != null
                ? PlayerProgression.Instance.currentShip.id
                : "scout";

            int finalScore = RunSummary.CalculateScore(GemsCollectedInRun, AsteroidsMinedCount, EnemiesDestroyedCount, false, RunTimer);
            StorageService.SaveHighScore(finalScore);
            StorageService.AddMetaGems(GemsCollectedInRun);

            var summary = new RunSummary
            {
                isVictory = false,
                timeSurvived = RunTimer,
                gemsCollected = GemsCollectedInRun,
                asteroidsMined = AsteroidsMinedCount,
                enemiesDestroyed = EnemiesDestroyedCount,
                bossDefeated = false,
                finalShipId = shipId,
                finalScore = finalScore
            };

            LastRunSummary = summary;
            ChangeState(GameState.GameOver);
            GameEvents.TriggerGameOver(summary);
        }

        public void Victory()
        {
            if (CurrentState != GameState.Playing) return;

            string shipId = PlayerProgression.Instance != null && PlayerProgression.Instance.currentShip != null
                ? PlayerProgression.Instance.currentShip.id
                : "scout";

            int finalScore = RunSummary.CalculateScore(GemsCollectedInRun, AsteroidsMinedCount, EnemiesDestroyedCount, true, RunTimer);
            StorageService.SaveHighScore(finalScore);
            StorageService.AddMetaGems(GemsCollectedInRun);

            var summary = new RunSummary
            {
                isVictory = true,
                timeSurvived = RunTimer,
                gemsCollected = GemsCollectedInRun,
                asteroidsMined = AsteroidsMinedCount,
                enemiesDestroyed = EnemiesDestroyedCount,
                bossDefeated = true,
                finalShipId = shipId,
                finalScore = finalScore
            };

            LastRunSummary = summary;
            ChangeState(GameState.Victory);
            GameEvents.TriggerVictory(summary);
        }

        public void RestartGame()
        {
            ChangeState(GameState.Boot);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
            Time.timeScale = 1f;
        }
    }
}
