using UnityEditor;
using UnityEngine;
using System.IO;

namespace StarStrike.Editor
{
    public static class CIBuilder
    {
        private static string[] GetScenes()
        {
            return new string[] { "Assets/_Project/Scenes/Game.unity" };
        }

        [MenuItem("Build/Windows")]
        public static void BuildWindows()
        {
            string path = "Builds/Windows/StarStrike.exe";
            BuildPipeline.BuildPlayer(GetScenes(), path, BuildTarget.StandaloneWindows64, BuildOptions.None);
            Debug.Log("Built Windows at " + path);
        }

        [MenuItem("Build/Android")]
        public static void BuildAndroid()
        {
            string path = "Builds/Android/StarStrike.apk";
            BuildPipeline.BuildPlayer(GetScenes(), path, BuildTarget.Android, BuildOptions.None);
            Debug.Log("Built Android at " + path);
        }

        [MenuItem("Build/WebGL")]
        public static void BuildWebGL()
        {
            string path = "Builds/WebGL";
            BuildPipeline.BuildPlayer(GetScenes(), path, BuildTarget.WebGL, BuildOptions.None);
            Debug.Log("Built WebGL at " + path);
        }

        [MenuItem("Build/iOS")]
        public static void BuildIOS()
        {
            string path = "Builds/iOS";
            BuildPipeline.BuildPlayer(GetScenes(), path, BuildTarget.iOS, BuildOptions.None);
            Debug.Log("Built iOS at " + path);
        }

        // CI hook
        public static void BuildAll()
        {
            BuildWindows();
            BuildAndroid();
            BuildWebGL();
            // BuildIOS(); // Optional: Often fails on non-Mac CI
        }
    }
}
