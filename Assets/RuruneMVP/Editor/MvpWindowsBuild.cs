using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RuruneMVP.Editor
{
    public static class MvpWindowsBuild
    {
        [MenuItem("Tools/Rurune MVP/Build Windows App")]
        public static void Build()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer)
            {
                Debug.LogError("Stop Play Mode and any current build before building the app.");
                return;
            }
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded || EditorSceneManager.IsPreviewScene(scene) ||
                !scene.path.StartsWith("Assets/LocalOnly/", StringComparison.Ordinal))
            {
                Debug.LogError("Open the configured app scene saved under Assets/LocalOnly before building.");
                return;
            }
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64))
            {
                Debug.LogError("Windows Build Support is unavailable. Add the appropriate Windows Build Support module " +
                    "for this Editor in Unity Hub. Keep the model's required Editor version.");
                return;
            }
            var orbits = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<OrbitCamera>(false))
                .Where(item => item.enabled).ToArray();
            if (orbits.Length != 1 || !orbits[0].GetComponent<Camera>().isActiveAndEnabled ||
                !orbits[0].CompareTag("MainCamera"))
            {
                Debug.LogError("The app scene needs one enabled OrbitCamera with an enabled Camera and the MainCamera tag.");
                return;
            }
            var panels = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<MvpControlPanel>(false))
                .Where(item => item.enabled).ToArray();
            if (panels.Length != 1)
            {
                Debug.LogError("The app scene needs one enabled MVP Control Panel. Use Quick Setup to connect its references.");
                return;
            }
            if (!EditorSceneManager.SaveScene(scene))
            {
                Debug.LogError("Scene could not be saved; build cancelled.");
                return;
            }
            string project = Directory.GetParent(Application.dataPath).FullName;
            string baseDirectory = Path.Combine(project, "Builds", "Windows", "Mvp_" + DateTime.Now.ToString("yyyyMMdd_HHmmss"));
            string directory = baseDirectory;
            for (int suffix = 1; Directory.Exists(directory) || File.Exists(directory); suffix++)
                directory = baseDirectory + "_" + suffix;
            Directory.CreateDirectory(directory);
            var options = new BuildPlayerOptions
            {
                scenes = new[] { scene.path },
                locationPathName = Path.Combine(directory, "RuruneApp.exe"),
                targetGroup = BuildTargetGroup.Standalone,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            };
            try
            {
                // Build Settings' scene list and Player Settings are not rewritten.
                // Only the active app scene is an entry point; Unity still includes its dependencies,
                // Resources and StreamingAssets according to normal build rules.
                BuildReport report = BuildPipeline.BuildPlayer(options);
                string summary = "Windows build: " + report.summary.result + "; errors=" + report.summary.totalErrors +
                    "; warnings=" + report.summary.totalWarnings + "; bytes=" + report.summary.totalSize;
                File.WriteAllText(Path.Combine(directory, "BuildResult.txt"), summary + Environment.NewLine +
                    "Unity: " + Application.unityVersion + Environment.NewLine +
                    "Entry scene: " + scene.path + Environment.NewLine +
                    "Local development build. Rendering and interaction still need testing on Windows.");
                if (report.summary.result != BuildResult.Succeeded)
                {
                    Debug.LogError(summary + ". Inspect Console errors. Output folder may contain incomplete files.");
                    return;
                }
                Debug.Log(summary + ". Output: " + directory + ". Run RuruneApp.exe locally to validate.");
                EditorUtility.RevealInFinder(options.locationPathName);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                Debug.LogError("Windows build did not complete. Inspect Console. Incomplete output was retained for diagnosis.");
            }
        }
    }
}
