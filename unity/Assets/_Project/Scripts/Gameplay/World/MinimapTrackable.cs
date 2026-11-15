using System;
using UnityEngine;

namespace StarStrike.Gameplay
{
    public enum TrackableType
    {
        Player,
        Enemy,
        Asteroid,
        Gem
    }

    public class MinimapTrackable : MonoBehaviour
    {
        public TrackableType Type = TrackableType.Enemy;

        public static event Action<MinimapTrackable> OnTrackableEnabled;
        public static event Action<MinimapTrackable> OnTrackableDisabled;

        public void SetType(TrackableType type)
        {
            if (Type != type)
            {
                Type = type;
                if (isActiveAndEnabled)
                {
                    OnTrackableDisabled?.Invoke(this);
                    OnTrackableEnabled?.Invoke(this);
                }
            }
        }

        private void OnEnable()
        {
            OnTrackableEnabled?.Invoke(this);
        }

        private void OnDisable()
        {
            OnTrackableDisabled?.Invoke(this);
        }
    }
}
