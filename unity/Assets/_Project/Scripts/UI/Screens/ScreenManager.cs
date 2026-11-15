using System;
using System.Collections.Generic;
using UnityEngine;

namespace StarStrike.UI
{
    /// <summary>
    /// Centralized navigation and lifecycle manager for decoupled UI screens.
    /// Eliminates direct hierarchy coupling, sorting collisions, and raycast blocking.
    /// </summary>
    public class ScreenManager : MonoBehaviour
    {
        public static ScreenManager Instance { get; private set; }

        private readonly Dictionary<string, IScreenController> registeredScreens = new Dictionary<string, IScreenController>(StringComparer.OrdinalIgnoreCase);
        private readonly Stack<string> modalStack = new Stack<string>();

        public string CurrentScreenId { get; private set; }

        public event Action<string> OnScreenShown;
        public event Action<string> OnScreenHidden;
        public event Action<string> OnModalPushed;
        public event Action<string> OnModalPopped;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>
        /// Registers a screen controller.
        /// </summary>
        public void RegisterScreen(IScreenController screen)
        {
            if (screen == null || string.IsNullOrEmpty(screen.ScreenId)) return;

            if (registeredScreens.ContainsKey(screen.ScreenId))
            {
                registeredScreens[screen.ScreenId] = screen;
            }
            else
            {
                registeredScreens.Add(screen.ScreenId, screen);
            }
        }

        /// <summary>
        /// Unregisters a screen controller.
        /// </summary>
        public void UnregisterScreen(IScreenController screen)
        {
            if (screen == null || string.IsNullOrEmpty(screen.ScreenId)) return;

            if (registeredScreens.TryGetValue(screen.ScreenId, out var existing) && existing == screen)
            {
                registeredScreens.Remove(screen.ScreenId);
                if (CurrentScreenId == screen.ScreenId)
                {
                    CurrentScreenId = null;
                }
            }
        }

        /// <summary>
        /// Shows the target screen, optionally hiding all other registered screens.
        /// </summary>
        public bool ShowScreen(string screenId, bool hideOthers = true)
        {
            if (string.IsNullOrEmpty(screenId)) return false;

            if (!registeredScreens.TryGetValue(screenId, out var targetScreen))
            {
                Debug.LogWarning($"[ScreenManager] Screen '{screenId}' is not registered.");
                return false;
            }

            if (hideOthers)
            {
                foreach (var kvp in registeredScreens)
                {
                    if (!string.Equals(kvp.Key, screenId, StringComparison.OrdinalIgnoreCase))
                    {
                        if (kvp.Value.IsVisible)
                        {
                            kvp.Value.Hide();
                            OnScreenHidden?.Invoke(kvp.Key);
                        }
                    }
                }
            }

            targetScreen.Show();
            CurrentScreenId = screenId;
            OnScreenShown?.Invoke(screenId);
            return true;
        }

        /// <summary>
        /// Hides the target screen.
        /// </summary>
        public bool HideScreen(string screenId)
        {
            if (string.IsNullOrEmpty(screenId)) return false;

            if (!registeredScreens.TryGetValue(screenId, out var targetScreen))
            {
                return false;
            }

            if (targetScreen.IsVisible)
            {
                targetScreen.Hide();
                if (string.Equals(CurrentScreenId, screenId, StringComparison.OrdinalIgnoreCase))
                {
                    CurrentScreenId = null;
                }
                OnScreenHidden?.Invoke(screenId);
            }
            return true;
        }

        /// <summary>
        /// Pushes a modal overlay on top of current screens.
        /// </summary>
        public bool PushModal(string screenId)
        {
            if (string.IsNullOrEmpty(screenId)) return false;

            if (!registeredScreens.TryGetValue(screenId, out var targetScreen))
            {
                Debug.LogWarning($"[ScreenManager] Modal screen '{screenId}' is not registered.");
                return false;
            }

            modalStack.Push(screenId);
            targetScreen.Show();
            OnModalPushed?.Invoke(screenId);
            return true;
        }

        /// <summary>
        /// Pops the topmost modal from the stack.
        /// </summary>
        public bool PopModal()
        {
            if (modalStack.Count == 0) return false;

            string topScreenId = modalStack.Pop();
            if (registeredScreens.TryGetValue(topScreenId, out var targetScreen))
            {
                targetScreen.Hide();
                OnModalPopped?.Invoke(topScreenId);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks whether a given screen is currently visible.
        /// </summary>
        public bool IsScreenVisible(string screenId)
        {
            if (string.IsNullOrEmpty(screenId)) return false;
            return registeredScreens.TryGetValue(screenId, out var screen) && screen.IsVisible;
        }

        /// <summary>
        /// Retrieves the typed screen controller if registered.
        /// </summary>
        public T GetScreen<T>(string screenId) where T : class, IScreenController
        {
            if (string.IsNullOrEmpty(screenId)) return null;
            if (registeredScreens.TryGetValue(screenId, out var screen))
            {
                return screen as T;
            }
            return null;
        }

        /// <summary>
        /// Clears all registrations (e.g., when reloading or transitioning scenes).
        /// </summary>
        public void ClearRegistrations()
        {
            modalStack.Clear();
            registeredScreens.Clear();
            CurrentScreenId = null;
        }
    }
}
