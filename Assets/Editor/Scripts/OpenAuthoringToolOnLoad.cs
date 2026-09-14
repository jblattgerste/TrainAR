using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Editor.Scripts
{
    /// <summary>
    /// Sets up a new checkout after its assets and Visual Scripting have finished importing.
    /// Subsequent editor sessions retain the user's scene, layout and build target.
    /// </summary>
    [InitializeOnLoad]
    public class OpenAuthoringToolOnLoad : AssetPostprocessor
    {
        private const string AttemptedKey = "TrainAR.Startup.Attempted";
        private const string PlatformAttemptedKey = "TrainAR.Startup.PlatformAttempted";

        static OpenAuthoringToolOnLoad()
        {
            // Register only a callback here: assets must not be loaded from InitializeOnLoad.
            // This also resumes platform switches that reload scripts without importing assets.
            if (!Application.isBatchMode)
                EditorApplication.update += WaitForEditor;
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            // The final import callback is not always marked as a domain reload on a new checkout.
            if (Application.isBatchMode || TrainARStartupState.instance.completed)
                return;

            EditorApplication.update -= WaitForEditor;
            EditorApplication.update += WaitForEditor;
        }

        private static void WaitForEditor()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating || !PluginContainer.initialized)
                return;

            // The first asset import can finish before Unity has restored its initial scene.
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded)
                return;

            EditorApplication.update -= WaitForEditor;
            OpenAuthoringTool();
        }

        private static void OpenAuthoringTool()
        {
            if (TrainARStartupState.instance.completed || SessionState.GetBool(AttemptedKey, false))
                return;

            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.update += WaitForEditor;
                return;
            }

            // Never replace an existing scene, unsaved edits, or a prefab stage.
            if (EditorApplication.isPlayingOrWillChangePlaymode ||
                PrefabStageUtility.GetCurrentPrefabStage() != null || !IsUntouchedStartupScene())
            {
                TrainARStartupState.instance.Complete();
                return;
            }

            var target = EditorUserBuildSettings.activeBuildTarget;
            if (target != BuildTarget.Android && target != BuildTarget.iOS &&
                !SessionState.GetBool(PlatformAttemptedKey, false) &&
                BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            {
                SessionState.SetBool(PlatformAttemptedKey, true);
                if (EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android))
                {
                    // Switching platforms recompiles scripts. Resume after that import.
                    return;
                }
            }

            // Prevent a failing initialization from looping; leave diagnostics visible.
            SessionState.SetBool(AttemptedKey, true);
            TrainAREditorMenu.SwitchToTrainARMode();
            if (SceneManager.GetActiveScene().path == "Assets/Scene.unity")
                TrainARStartupState.instance.Complete();
        }

        private static bool IsUntouchedStartupScene()
        {
            if (SceneManager.sceneCount != 1)
                return false;

            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !string.IsNullOrEmpty(scene.path) || scene.isDirty)
                return false;

            // Unity's default Untitled scene can contain a camera and directional light.
            foreach (var root in scene.GetRootGameObjects())
            {
                if (root.name == "Main Camera" && root.GetComponent<Camera>() != null)
                    continue;
                if (root.name == "Directional Light" && root.GetComponent<Light>() != null)
                    continue;
                // Restoring a Visual Scripting window can create this empty helper in Untitled.
                if (root.name == "VisualScripting SceneVariables" &&
                    root.TryGetComponent<SceneVariables>(out var variables) &&
                    !variables.variables.declarations.Any() && root.transform.childCount == 0 &&
                    root.GetComponents<Component>().Length == 3)
                    continue;
                return false;
            }
            return true;
        }
    }

    [FilePath("UserSettings/TrainARStartup.asset", FilePathAttribute.Location.ProjectFolder)]
    internal class TrainARStartupState : ScriptableSingleton<TrainARStartupState>
    {
        [SerializeField] internal bool completed;

        internal void Complete()
        {
            completed = true;
            Save(true);
        }
    }
}
