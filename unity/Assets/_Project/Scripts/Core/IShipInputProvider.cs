using UnityEngine;

namespace StarStrike.Core
{
    /// <summary>
    /// Contract for entities providing movement and directional input to a ship.
    /// </summary>
    public interface IShipInputProvider
    {
        Vector2 AimDirection { get; }
        float TurnInput { get; }
        bool IsThrusting { get; }
        bool IsBoosting { get; }
        bool IsBraking { get; }
    }
}
