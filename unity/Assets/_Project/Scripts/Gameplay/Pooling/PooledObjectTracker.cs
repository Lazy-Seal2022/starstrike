using UnityEngine;

namespace StarStrike.Gameplay
{
    /// <summary>
    /// Tracks pooled object lifecycle and notifies PoolManager on deactivation.
    /// Ensures O(1) recycling even when objects are deactivated via SetActive(false).
    /// </summary>
    [ExecuteAlways]
    public class PooledObjectTracker : MonoBehaviour
    {
        public PoolManager poolManager;
        public string poolTag;
        public bool isSpawned;

        private void OnDisable()
        {
            if (isSpawned)
            {
                var mgr = poolManager != null ? poolManager : PoolManager.Instance;
                if (mgr != null)
                {
                    mgr.NotifyDeactivated(gameObject, poolTag);
                }
            }
        }
    }
}
