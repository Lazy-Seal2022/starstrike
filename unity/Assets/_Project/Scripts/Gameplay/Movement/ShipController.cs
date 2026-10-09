using UnityEngine;
using UnityEditor;

namespace StarStrike.Gameplay
{
#if UNITY_EDITOR
    public class ShipControllerMigrator
    {
        [MenuItem("StarStrike/Migrate ShipController")]
        public static void Migrate()
        {
            var ships = Object.FindObjectsByType<ShipController>(FindObjectsSortMode.None);
            foreach (var ship in ships)
            {
                var go = ship.gameObject;
                
                var movement = go.AddComponent<ShipMovement>();
                var health = go.AddComponent<ShipHealth>();
                health.isPlayer = true;
                
                var weapon = go.AddComponent<ShipWeapon>();
                weapon.isPlayer = true;
                
                var prog = go.AddComponent<PlayerProgression>();
                prog.health = health;
                prog.movement = movement;
                prog.weapon = weapon;
                prog.shipRenderer = go.GetComponent<SpriteRenderer>();

                var input = go.AddComponent<PlayerInputReader>();
                input.movement = movement;
                input.weapon = weapon;
                input.health = health;

                Object.DestroyImmediate(ship);
                EditorUtility.SetDirty(go);
                Debug.Log($"Migrated {go.name}");
            }

            var ais = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
            foreach (var ai in ais)
            {
                var go = ai.gameObject;
                if (go.GetComponent<ShipMovement>() == null) go.AddComponent<ShipMovement>();
                if (go.GetComponent<ShipHealth>() == null) go.AddComponent<ShipHealth>();
                if (go.GetComponent<ShipWeapon>() == null) go.AddComponent<ShipWeapon>();
                EditorUtility.SetDirty(go);
                Debug.Log($"Migrated AI {go.name}");
            }

            UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
        }
    }
#endif

    public class ShipController : MonoBehaviour
    {
        // Legacy class kept only for migration
    }
}
