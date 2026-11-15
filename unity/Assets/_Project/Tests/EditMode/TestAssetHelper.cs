using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace StarStrike.Tests.EditMode
{
    public static class TestAssetHelper
    {
        public static GameObject LoadPrefab(string relativePath)
        {
#if UNITY_EDITOR
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Project/Prefabs/{relativePath}.prefab");
            if (prefab != null) return prefab;
#endif
            return Resources.Load<GameObject>($"Prefabs/{relativePath}");
        }
    }
}
