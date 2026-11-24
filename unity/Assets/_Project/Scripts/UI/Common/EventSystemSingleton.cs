using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace StarStrike.UI
{
    /// <summary>
    /// Enforces exactly one EventSystem across the entire application lifecycle,
    /// preventing duplicate EventSystem warnings when loading new scenes.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    [DisallowMultipleComponent]
    public class EventSystemSingleton : MonoBehaviour
    {
        private void Awake()
        {
            EnforceSingleEventSystem();
        }

        private void OnEnable()
        {
            EnforceSingleEventSystem();
        }

        private void Update()
        {
            EnforceSingleEventSystem();
        }

        /// <summary>
        /// Scans all active EventSystems in the scene/application, preserves exactly one
        /// (preferring one marked DontDestroyOnLoad), and immediately destroys any duplicates.
        /// </summary>
        public static EventSystem EnforceSingleEventSystem()
        {
            var systems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            if (systems == null || systems.Length == 0)
            {
                var eventSystemGO = new GameObject("EventSystem");
                var es = eventSystemGO.AddComponent<EventSystem>();
                eventSystemGO.AddComponent<InputSystemUIInputModule>();
                eventSystemGO.AddComponent<EventSystemSingleton>();
                if (Application.isPlaying)
                {
                    DontDestroyOnLoad(eventSystemGO);
                }
                return es;
            }

            EventSystem keeper = null;
            // Prefer an EventSystem that is already in DontDestroyOnLoad
            for (int i = 0; i < systems.Length; i++)
            {
                var es = systems[i];
                if (es == null) continue;
                if (es.gameObject.scene.name == "DontDestroyOnLoad")
                {
                    keeper = es;
                    break;
                }
            }

            // Fallback to the first valid one if none are in DontDestroyOnLoad
            if (keeper == null)
            {
                keeper = systems[0];
            }

            // Destroy all other EventSystems
            for (int i = 0; i < systems.Length; i++)
            {
                var es = systems[i];
                if (es != null && es != keeper)
                {
                    if (Application.isPlaying)
                    {
                        Destroy(es.gameObject);
                    }
                    else
                    {
                        DestroyImmediate(es.gameObject);
                    }
                }
            }

            // Ensure keeper has EventSystemSingleton component
            if (keeper != null)
            {
                if (keeper.GetComponent<EventSystemSingleton>() == null)
                {
                    keeper.gameObject.AddComponent<EventSystemSingleton>();
                }
                if (keeper.GetComponent<InputSystemUIInputModule>() == null)
                {
                    keeper.gameObject.AddComponent<InputSystemUIInputModule>();
                }
                if (Application.isPlaying && keeper.transform.parent == null && keeper.gameObject.scene.name != "DontDestroyOnLoad")
                {
                    DontDestroyOnLoad(keeper.gameObject);
                }
            }

            return keeper;
        }
    }
}
