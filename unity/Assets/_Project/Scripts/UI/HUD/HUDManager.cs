using UnityEngine;

namespace StarStrike.UI
{
    /// <summary>
    /// Legacy bootstrap shim preserving scene compatibility by delegating directly to CanvasHUD.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        [Tooltip("When enabled, migrates automatically to modern uGUI CanvasHUD.")]
        public bool useCanvasUI = true;

        private void Awake()
        {
            if (useCanvasUI)
            {
                if (GetComponent<CanvasHUD>() == null && FindAnyObjectByType<CanvasHUD>() == null)
                {
                    gameObject.AddComponent<CanvasHUD>();
                }
                enabled = false;
            }
        }
    }
}
