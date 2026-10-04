using System.IO;
using System.Linq;
using Unity.PlayMode.Editor;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Editor.Scripts
{
    /// <summary>
    /// The PlayModeButtonOverride editor script implements utility for the unity editor play buttons that overrides
    /// its functionality with TrainAR specific functionality to either switch the build target if there is an
    /// unsupported one currently selected or allow building to a device by clicking the play button.
    ///
    /// This is done for convenience of building and also to prevent the playmode execution, as this is
    /// currently not supported by the framework.
    /// </summary>
    [InitializeOnLoad] //Ensure class initializer is called whenever scripts recompile in Editor
    public static class PlayModeButtonOverride
    {
        private const string PendingBuildPromptKey = "TrainAR.PlayMode.PendingBuildPrompt";
        private static bool workflowRunning;
        private static bool observedIdleUpdate;
        
        //Register an event handler when the class is initialized
        static PlayModeButtonOverride()
        {
            if (Application.isBatchMode) return;
            EditorApplication.delayCall += RegisterPlayModeOverride;
        }

        private static void RegisterPlayModeOverride()
        {
            // Unity 6.7's scenario must observe ExitingEditMode before we cancel
            // it. Otherwise its outer callback overwrites the nested cancellation
            // with Starting, leaving the toolbar stuck on Stop. Initialize it and
            // subscribe after its reload/OnEnable callbacks have completed.
            _ = PlayModeScenarioManager.ActiveScenario;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            if (SessionState.GetBool(PendingBuildPromptKey, false))
                ScheduleBuildPrompt();
        }

        /// <summary>
        /// On trying to exit the edit mode, right before entering playmode, catch this and prohibit it.
        ///
        /// Show windows to either switch the platform or building.
        /// </summary>
        /// <param name="state">The Playmode state</param>
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode) return;

            // Cancel before scene startup, then let all Play callbacks unwind.
            // Modal dialogs, platform switches and builds run later in Edit Mode.
            EditorApplication.isPlaying = false;
            ShowDialogBoxForBuilding();
        }

        private static bool EditorIsBusy()
        {
            return EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode ||
                   PlayModeScenarioManager.State != PlayModeScenarioState.Idle ||
                   EditorApplication.isCompiling || EditorApplication.isUpdating || BuildPipeline.isBuildingPlayer;
        }

        private static void ScheduleBuildPrompt()
        {
            observedIdleUpdate = false;
            EditorApplication.update -= WaitForEditMode;
            EditorApplication.update += WaitForEditMode;
        }

        private static void WaitForEditMode()
        {
            if (!SessionState.GetBool(PendingBuildPromptKey, false))
            {
                EditorApplication.update -= WaitForEditMode;
                return;
            }
            if (workflowRunning || EditorIsBusy())
            {
                observedIdleUpdate = false;
                return;
            }
            // Allow an entire idle update to pass before opening a modal. This
            // also lets Unity's Play toolbar finish processing the cancellation.
            if (!observedIdleUpdate)
            {
                observedIdleUpdate = true;
                return;
            }

            EditorApplication.update -= WaitForEditMode;
            SessionState.SetBool(PendingBuildPromptKey, false);
            workflowRunning = true;
            try
            {
                var target = EditorUserBuildSettings.activeBuildTarget;
                if (target == BuildTarget.Android || target == BuildTarget.iOS)
                    ShowBuildDialogInEditMode();
                else
                    ShowDialogBoxForSwitchingPlatform(target);
            }
            finally
            {
                workflowRunning = false;
            }
        }
        
        /// <summary>
        /// Shows a Unity Editor Dialog box asking the user if he wants to build the Project.
        /// </summary>
        public static void ShowDialogBoxForBuilding()
        {
            if (Application.isBatchMode || workflowRunning || BuildPipeline.isBuildingPlayer) return;
            SessionState.SetBool(PendingBuildPromptKey, true);
            ScheduleBuildPrompt();
        }

        private static void ShowBuildDialogInEditMode()
        {
            //Show a Dialog Box for building
            if (EditorUtility.DisplayDialog("Build TrainAR Project",
                "Do you want to build this TrainAR project to a device?",
                "Build",
                "Cancel"))
            {
                //This means, the user clicked "Build", build and run the app
                if (!BuildAndDeployProjectToDevice())
                {
                    //If the build failed, inform the user to consult the console window
                    EditorUtility.DisplayDialog("Build failed!",
                        "The current build failed. See the \"Console\" window for more details.","Ok");
                }
                else
                {
                    //After the build was successful, reset the TrainAR scene
                    TrainAREditorMenu.ResetTrainARSceneToAuthoringToolDefault();
                }
            }
            else
            {
                //This means, the user clicked "Cancel" and we do nothing
            }
        }

        /// <summary>
        /// Shows a Unity Editor Dialog box asking the user if he wants to switch platforms if the wrong one is selected.
        /// </summary>
        /// <param name="currentBuildTarget">the currently selected build target platform</param>
        private static void ShowDialogBoxForSwitchingPlatform(BuildTarget currentBuildTarget)
        {
            //Show a Dialog Box for switching the platform first
            bool useAndroid = EditorUtility.DisplayDialog("Switch Build Targets",
                "The current build target of the project is set to " +
                currentBuildTarget +
                " which is not supported by TrainAR. Do you want to deploy to an Android or iOS device?",
                "Android",
                "iOS");
            var target = useAndroid ? BuildTarget.Android : BuildTarget.iOS;
            var group = useAndroid ? BuildTargetGroup.Android : BuildTargetGroup.iOS;

            // A target switch can reload this class. Persist the continuation
            // before switching and resume it only after compilation/import ends.
            SessionState.SetBool(PendingBuildPromptKey, true);
            try
            {
                if (EditorUserBuildSettings.SwitchActiveBuildTarget(group, target))
                {
                    ScheduleBuildPrompt();
                    return;
                }
                SessionState.SetBool(PendingBuildPromptKey, false);
                Debug.LogError("TrainAR could not switch the build target to " + target + ".");
            }
            catch
            {
                SessionState.SetBool(PendingBuildPromptKey, false);
                throw;
            }
        }

        /// <summary>
        /// Build and Runs the project for the current build target.
        /// </summary>
        /// <returns>True, if the build succeeded</returns>
        public static bool BuildAndDeployProjectToDevice()
        {
            if (EditorIsBusy())
            {
                Debug.LogError("TrainAR can only build after the Editor has returned to idle Edit Mode.");
                return false;
            }
            var target = EditorUserBuildSettings.activeBuildTarget;
            if (target != BuildTarget.Android && target != BuildTarget.iOS)
            {
                Debug.LogError("TrainAR builds support Android and iOS only.");
                return false;
            }

            var outputDirectory = target == BuildTarget.Android ? "Builds/Android" : "Builds/iOS";
            Directory.CreateDirectory(outputDirectory);

            //Create new settings for this build
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                //Search for all the scenes currently active in the EditorBuildSettings, get their path and include them
                scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
                //Android creates an APK; iOS exports a project directory for Xcode.
                locationPathName = target == BuildTarget.Android
                    ? Path.Combine(outputDirectory, "TrainAR.apk")
                    : outputDirectory,
                //Build for the currently selected target
                target = target,
                //Set the options to the "build and run" equivalent
                options = BuildOptions.AutoRunPlayer
            };

            bool wasAppBundle = EditorUserBuildSettings.buildAppBundle;
            bool wasAndroidProjectExport = EditorUserBuildSettings.exportAsGoogleAndroidProject;
            try
            {
                if (target == BuildTarget.Android)
                {
                    EditorUserBuildSettings.buildAppBundle = false;
                    EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
                }

                var buildReportData = BuildPipeline.BuildPlayer(buildPlayerOptions);
                return buildReportData.summary.result == BuildResult.Succeeded;
            }
            finally
            {
                EditorUserBuildSettings.buildAppBundle = wasAppBundle;
                EditorUserBuildSettings.exportAsGoogleAndroidProject = wasAndroidProjectExport;
            }
        }

        /// <summary>
        /// Switches Unity Build Target to iOS.
        /// </summary>
        public static void SwitchBuildTargetToIOS()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
        }
        
        /// <summary>
        /// Switches Unity Build Target to Android.
        /// </summary>
        public static void SwitchBuildTargetToAndroid()
        {
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }
        
        
    }
}
