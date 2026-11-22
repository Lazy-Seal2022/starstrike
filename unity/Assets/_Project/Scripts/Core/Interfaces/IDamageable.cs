namespace StarStrike.Core
{
    public interface IDamageable
    {
        void TakeDamage(float amount);
        void TakeDamage(float amount, UnityEngine.Vector2 hitPoint) => TakeDamage(amount);
    }
}
