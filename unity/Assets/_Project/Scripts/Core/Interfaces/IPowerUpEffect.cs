using UnityEngine;

namespace StarStrike.Core
{
    /// <summary>
    /// Polymorphic effect strategy applied when collecting power-ups.
    /// </summary>
    public interface IPowerUpEffect
    {
        string Name { get; }
        Color EffectColor { get; }
        void Apply(GameObject target, LootConfig config);
    }
}
