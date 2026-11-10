using System.Collections;
using UnityEngine;

namespace StarStrike.Core
{
    public static class Ease
    {
        public static float Linear(float t) => Mathf.Clamp01(t);

        public static float OutQuad(float t)
        {
            t = Mathf.Clamp01(t);
            return t * (2f - t);
        }

        public static float OutCubic(float t)
        {
            t = Mathf.Clamp01(t);
            float f = t - 1f;
            return f * f * f + 1f;
        }

        public static float OutBack(float t)
        {
            t = Mathf.Clamp01(t);
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            return 1f + c3 * Mathf.Pow(t - 1f, 3) + c1 * Mathf.Pow(t - 1f, 2);
        }

        public static IEnumerator Tween(float duration, System.Action<float> apply, System.Func<float, float> ease, bool unscaled = true)
        {
            if (duration <= 0f)
            {
                apply?.Invoke(1f);
                yield break;
            }

            for (float t = 0f; t < 1f; t += (unscaled ? Time.unscaledDeltaTime : Time.deltaTime) / duration)
            {
                apply?.Invoke(ease != null ? ease(Mathf.Clamp01(t)) : Mathf.Clamp01(t));
                yield return null;
            }

            apply?.Invoke(1f);
        }
    }
}
