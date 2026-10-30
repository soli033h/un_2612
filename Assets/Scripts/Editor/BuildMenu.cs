using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Racing.EditorTools
{
    /// <summary>
    /// One-click builds. Also callable from the command line:
    /// Unity.exe -batchmode -quit -projectPath . -executeMethod Racing.EditorTools.BuildMenu.BuildWindowsClient
    /// </summary>
    public static class BuildMenu
    {
        const string WindowsClientPath = "Builds/Windows/un_2612.exe";

        [MenuItem("Racing/Build Windows Client")]
        public static void BuildWindowsClient()
        {
            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
                locationPathName = WindowsClientPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildSummary summary = BuildPipeline.BuildPlayer(options).summary;
            if (summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"Windows client build {summary.result} with {summary.totalErrors} error(s).");
                if (Application.isBatchMode)
                    EditorApplication.Exit(1);
                return;
            }

            Debug.Log($"Windows client built to {WindowsClientPath} ({summary.totalSize / (1024 * 1024)} MB, {summary.totalTime.TotalSeconds:0}s).");
            if (!Application.isBatchMode)
                EditorUtility.RevealInFinder(WindowsClientPath);
        }
    }
}
