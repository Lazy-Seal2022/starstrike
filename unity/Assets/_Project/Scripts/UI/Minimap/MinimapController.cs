using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using StarStrike.Gameplay;

namespace StarStrike.UI
{
    public class MinimapController : MonoBehaviour
    {
        public static MinimapController Instance { get; private set; }

        [Header("Settings")]
        public float radarRange = 50f; // World units
        public float mapSize = 150f; // UI units (width/height of the radar)

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
            MinimapTrackable.OnTrackableEnabled += RegisterTrackable;
            MinimapTrackable.OnTrackableDisabled += UnregisterTrackable;
        }

        private void OnDisable()
        {
            MinimapTrackable.OnTrackableEnabled -= RegisterTrackable;
            MinimapTrackable.OnTrackableDisabled -= UnregisterTrackable;
        }

        private void BuildUI()
        {
            if (mapContainer == null)
            {
                GameObject bgObj = new GameObject("Minimap_BG", typeof(RectTransform), typeof(Image));
                bgObj.transform.SetParent(transform, false);
                mapContainer = bgObj.GetComponent<RectTransform>();
                
                mapContainer.anchorMin = new Vector2(1, 1);
                mapContainer.anchorMax = new Vector2(1, 1);
                mapContainer.pivot = new Vector2(1, 1);
                mapContainer.anchoredPosition = new Vector2(-20, -20);
                mapContainer.sizeDelta = new Vector2(mapSize, mapSize);

                background = bgObj.GetComponent<Image>();
                background.color = new Color(0, 0, 0, 0.5f);
            }
        }

        public void RegisterTrackable(MinimapTrackable trackable)
        {
            if (!trackables.Contains(trackable))
            {
                trackables.Add(trackable);
                
                if (trackable.Type == TrackableType.Player && playerTransform == null)
                {
                    playerTransform = trackable.transform;
                }
            }
        }

        public void UnregisterTrackable(MinimapTrackable trackable)
        {
            if (trackables.Contains(trackable))
            {
                trackables.Remove(trackable);
                if (trackableDots.ContainsKey(trackable))
                {
                    if (trackableDots[trackable] != null)
                        Destroy(trackableDots[trackable].gameObject);
                    trackableDots.Remove(trackable);
                }
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

            foreach (var trackable in trackables)
            {
                if (trackable == null) continue;

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

                        float maxDist = mapSize / 2f;
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
            GameObject dotObj = new GameObject("Dot_" + trackable.Type, typeof(RectTransform), typeof(Image));
            dotObj.transform.SetParent(mapContainer, false);
            
            RectTransform rt = dotObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            
            float dotSize = trackable.Type == TrackableType.Player ? 6f : 4f;
            if (trackable.Type == TrackableType.Asteroid) dotSize = 3f;
            rt.sizeDelta = new Vector2(dotSize, dotSize);

            Image img = dotObj.GetComponent<Image>();
            switch (trackable.Type)
            {
                case TrackableType.Player: img.color = playerColor; break;
                case TrackableType.Enemy: img.color = enemyColor; break;
                case TrackableType.Asteroid: img.color = asteroidColor; break;
                case TrackableType.Gem: img.color = gemColor; break;
            }

            return rt;
        }
    }
}
