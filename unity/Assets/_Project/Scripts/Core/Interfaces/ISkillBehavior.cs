using UnityEngine;

namespace StarStrike.Core
{
    public interface ISkillBehavior
    {
        void Initialize(GameObject owner, SkillDefinition definition);
        bool CanExecute(GameObject owner, SkillDefinition definition);
        void Execute(GameObject owner, Vector2 origin, Vector2 direction);
        void Terminate();
    }
}
