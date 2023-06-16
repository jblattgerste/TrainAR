using System.Linq;
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
        private static bool weJustSwitchedPlatform = false;
        
        //Register an event handler when the class is initialized
        static PlayModeButtonOverride()
        {
            //EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        /// <summary>
        /// On trying to exit the edit mode, right before entering playmode, catch this and prohibit it.
        ///
        /// Show windows to either switch the platform or building.
        /// </summary>
        /// <param name="state">The Playmode state</param>
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            //Right before entering playmode, while exiting editmode, kill the playmode execution
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                EditorApplication.isPlaying = false;

                //get the current build target
                BuildTarget currentBuildTarget = EditorUserBuildSettings.activeBuildTarget;

                //Check which buildtarget is currently selected
                if (currentBuildTarget == BuildTarget.Android || currentBuildTarget == BuildTarget.iOS)
                {
                    ShowDialogBoxForBuilding();
                }
                else
                {
                    ShowDialogBoxForSwitchingPlatform(currentBuildTarget);
                }
            }
            else if(state == PlayModeStateChange.ExitingPlayMode)
            {
                //This is also triggered with "exitingPlayMode" right after canceling the playmode execution so if we
                //just switched the platform immediately also trigger asking the user to build again
                if (!weJustSwitchedPlatform) return;
                ShowDialogBoxForBuilding();
                weJustSwitchedPlatform = false;
            }
            else
            {
                //Nothing.
            }
        }

        
        /// <summary>
        /// Shows a Unity Editor Dialog box asking the user if he wants to build the Project.
        /// </summary>
        public static void ShowDialogBoxForBuilding()
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
            if (EditorUtility.DisplayDialog("Switch Build Targets",
                "The current build target of the project is set to " +
                currentBuildTarget +
                " which is not supported by TrainAR. Do you want to deploy to an Android or iOS device?",
                "Android",
                "iOS"))
            {
                //This means, the user clicked "Android"
                SwitchBuildTargetToAndroid();
                weJustSwitchedPlatform = true;
            }
            else
            {
                //This means the user clicked "iOS"
                SwitchBuildTargetToIOS();
                weJustSwitchedPlatform = true;
            }
        }

        /// <summary>
        /// Build and Runs the project for the current build target.
        /// </summary>
        /// <returns>True, if the build succeeded</returns>
        public static bool BuildAndDeployProjectToDevice()
        {
            //Create new settings for this build
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                //Search for all the scenes currently active in the EditorBuildSettings, get their path and include them
                scenes = EditorBuildSettings.scenes.Select(scene => scene.path).ToArray(),
                //Store the build direclty into a "build" folder in the projects unity folder
                locationPathName = "build.apk",
                //Build for the currently selected target
                target = EditorUserBuildSettings.activeBuildTarget,
                //Set the options to the "build and run" equivalent
                options = BuildOptions.AutoRunPlayer
            };

            //Execute the build and store the report. Return false if it failed
            var buildReportData = BuildPipeline.BuildPlayer (buildPlayerOptions);
            return buildReportData.summary.result == BuildResult.Succeeded;
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
