using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using StarStrike.Core;
using StarStrike.Gameplay;

namespace StarStrike.UI
{
    public class MinimapController : MonoBehaviour
    {
        public static MinimapController Instance { get; private set; }

        [Header("Settings")]
        public float radarRange = 50f; // World units
        public float mapSize = 150f; // UI units (width/height of the radar)
        public float radarJitter = 0f;

        [Header("UI References")]
        public RectTransform mapContainer;
        public Image background;

        [Header("Dot Prefabs (Sprites or Colors)")]
        public Color playerColor = Color.green;
        public Color enemyColor = Color.red;
        public Color asteroidColor = new Color(0.6f, 0.6f, 0.6f);
        public Color gemColor = Color.cyan;

        private List<MinimapTrackable> trackables = new List<MinimapTrackable>();
        private Dictionary<MinimapTrackable, RectTransform> trackableDots = new Dictionary<MinimapTrackable, RectTransform>();
        private readonly Queue<RectTransform> dotPool = new Queue<RectTransform>();
        
        private Transform playerTransform;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            BuildUI();
        }

        private void OnEnable()
        {
            GameEvents.OnGameStarted += ClearAll;
            GameEvents.OnRadarJitterChanged += HandleRadarJitterChanged;
            MinimapTrackable.OnTrackableEnabled += RegisterTrackable;
            MinimapTrackable.OnTrackableDisabled += UnregisterTrackable;

            // Scan and register all currently active trackables
            var existing = FindObjectsByType<MinimapTrackable>(FindObjectsSortMode.None);
            foreach (var t in existing)
            {
                if (t != null && t.isActiveAndEnabled)
                {
                    RegisterTrackable(t);
                }
            }
        }

        private void OnDisable()
        {
            GameEvents.OnGameStarted -= ClearAll;
            GameEvents.OnRadarJitterChanged -= HandleRadarJitterChanged;
            MinimapTrackable.OnTrackableEnabled -= RegisterTrackable;
            MinimapTrackable.OnTrackableDisabled -= UnregisterTrackable;
        }

        private void HandleRadarJitterChanged(float jitter)
        {
            radarJitter = jitter;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void BuildUI()
        {
            RectTransform selfRt = GetComponent<RectTransform>();
            if (selfRt == null) selfRt = gameObject.AddComponent<RectTransform>();
            selfRt.anchorMin = Vector2.zero;
            selfRt.anchorMax = Vector2.one;
            selfRt.offsetMin = Vector2.zero;
            selfRt.offsetMax = Vector2.zero;

            if (mapContainer == null)
            {
                GameObject bgObj = new GameObject("Minimap_BG", typeof(RectTransform), typeof(Image));
                bgObj.transform.SetParent(transform, false);
                mapContainer = bgObj.GetComponent<RectTransform>();
                
                mapContainer.anchorMin = new Vector2(1f, 0f);
                mapContainer.anchorMax = new Vector2(1f, 0f);
                mapContainer.pivot = new Vector2(1f, 0f);
                mapContainer.anchoredPosition = new Vector2(-25f, 25f);
                mapContainer.sizeDelta = new Vector2(mapSize, mapSize);

                background = bgObj.GetComponent<Image>();
                background.sprite = ProceduralSpriteHelper.GetRadarSprite(128);
                background.color = Color.white;
            }
            else
            {
                mapContainer.anchorMin = new Vector2(1f, 0f);
                mapContainer.anchorMax = new Vector2(1f, 0f);
                mapContainer.pivot = new Vector2(1f, 0f);
                mapContainer.anchoredPosition = new Vector2(-25f, 25f);
                mapContainer.sizeDelta = new Vector2(mapSize, mapSize);
            }

            // Isolate minimap geometry into a nested Canvas so dynamic dot movement doesn't dirty the root HUD canvas
            Canvas subCanvas = mapContainer.GetComponent<Canvas>();
            if (subCanvas == null) subCanvas = mapContainer.gameObject.AddComponent<Canvas>();
            
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null && parentCanvas != subCanvas)
            {
                subCanvas.overrideSorting = true;
                subCanvas.sortingLayerID = parentCanvas.sortingLayerID;
                subCanvas.sortingOrder = parentCanvas.sortingOrder + 1;
            }

            CanvasGroup cg = mapContainer.GetComponent<CanvasGroup>();
            if (cg == null) cg = mapContainer.gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }

        public void RegisterTrackable(MinimapTrackable trackable)
        {
            if (trackable == null) return;

            if (!trackables.Contains(trackable))
            {
                trackables.Add(trackable);
                
                if (trackable.Type == TrackableType.Player && playerTransform == null)
                {
                    playerTransform = trackable.transform;
                }
            }

            if (!trackableDots.ContainsKey(trackable) && mapContainer != null)
            {
                trackableDots[trackable] = CreateDot(trackable);
            }
        }

        public void ClearAll()
        {
            foreach (var kvp in trackableDots)
            {
                if (kvp.Value != null)
                {
                    kvp.Value.gameObject.SetActive(false);
                    dotPool.Enqueue(kvp.Value);
                }
            }
            trackableDots.Clear();
            trackables.Clear();
        }

        public void UnregisterTrackable(MinimapTrackable trackable)
        {
            if (trackable == null) return;

            if (trackables.Contains(trackable))
            {
                trackables.Remove(trackable);
            }

            if (trackableDots.TryGetValue(trackable, out RectTransform dotRt))
            {
                if (dotRt != null)
                {
                    dotRt.gameObject.SetActive(false);
                    dotPool.Enqueue(dotRt);
                }
                trackableDots.Remove(trackable);
            }
        }

        private void LateUpdate()
        {
            if (playerTransform == null)
            {
                // Try to find player if not set
                GameObject p = GameObject.FindGameObjectWithTag("Player");
                if (p != null) playerTransform = p.transform;
                return;
            }

            Vector2 playerPos = new Vector2(playerTransform.position.x, playerTransform.position.y);

            for (int i = trackables.Count - 1; i >= 0; i--)
            {
                var trackable = trackables[i];
                if (trackable == null || !trackable.gameObject.activeInHierarchy)
                {
                    trackables.RemoveAt(i);
                    if (trackable != null && trackableDots.TryGetValue(trackable, out RectTransform deadDot))
                    {
                        if (deadDot != null)
                        {
                            deadDot.gameObject.SetActive(false);
                            dotPool.Enqueue(deadDot);
                        }
                        trackableDots.Remove(trackable);
                    }
                    continue;
                }

                if (!trackableDots.ContainsKey(trackable))
                {
                    trackableDots[trackable] = CreateDot(trackable);
                }

                RectTransform dotRt = trackableDots[trackable];
                if (dotRt != null)
                {
                    if (trackable.Type == TrackableType.Player)
                    {
                        dotRt.anchoredPosition = Vector2.zero;
                        dotRt.localRotation = Quaternion.Euler(0, 0, trackable.transform.eulerAngles.z);
                    }
                    else
                    {
                        Vector2 targetPos = new Vector2(trackable.transform.position.x, trackable.transform.position.y);
                        Vector2 offset = targetPos - playerPos;

                        float scale = (mapSize / 2f) / radarRange;
                        Vector2 uiPos = offset * scale;

                        if (radarJitter > 0.001f)
                        {
                            float noiseX = (Mathf.PerlinNoise(Time.time * 8f, i * 4.3f) - 0.5f) * radarJitter * 2f;
                            float noiseY = (Mathf.PerlinNoise(i * 4.3f, Time.time * 8f) - 0.5f) * radarJitter * 2f;
                            uiPos += new Vector2(noiseX, noiseY);
                        }

                        float maxDist = (mapSize / 2f) - 6f;
                        if (uiPos.magnitude > maxDist)
                        {
                            uiPos = uiPos.normalized * maxDist;
                        }
                        
                        dotRt.anchoredPosition = uiPos;
                        dotRt.localRotation = trackable.transform.localRotation;
                    }
                }
            }
        }

        private RectTransform CreateDot(MinimapTrackable trackable)
        {
            RectTransform rt = null;
            while (dotPool.Count > 0 && rt == null)
            {
                rt = dotPool.Dequeue();
            }

            if (rt == null)
            {
                GameObject dotObj = new GameObject("Dot_" + trackable.Type, typeof(RectTransform), typeof(Image));
                dotObj.transform.SetParent(mapContainer, false);
                rt = dotObj.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0.5f, 0.5f);
                rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = new Vector2(0.5f, 0.5f);
                
                Image img = dotObj.GetComponent<Image>();
                img.raycastTarget = false;
                img.sprite = ProceduralSpriteHelper.GetCircleSprite(16);
            }
            else
            {
                rt.gameObject.name = "Dot_" + trackable.Type;
                rt.gameObject.SetActive(true);
            }

            float dotSize = trackable.Type == TrackableType.Player ? 8f : 5f;
            if (trackable.Type == TrackableType.Asteroid) dotSize = 4f;
            rt.sizeDelta = new Vector2(dotSize, dotSize);

            Image dotImage = rt.GetComponent<Image>();
            if (dotImage != null)
            {
                switch (trackable.Type)
                {
                    case TrackableType.Player: dotImage.color = playerColor; break;
                    case TrackableType.Enemy: dotImage.color = enemyColor; break;
                    case TrackableType.Asteroid: dotImage.color = asteroidColor; break;
                    case TrackableType.Gem: dotImage.color = gemColor; break;
                }
            }

            return rt;
        }
    }
}
