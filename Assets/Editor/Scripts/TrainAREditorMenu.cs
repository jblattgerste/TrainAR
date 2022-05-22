using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Editor.Scripts
{
    /// <summary>
    /// The TrainAREditorMenu adds a top menu with options called "TrainAR" for trainAR specific funtionallity like
    /// building, switching plattform, switching the mode (TrainAR/Unity Editor), and other options.
    /// </summary>
    public class TrainAREditorMenu : UnityEditor.Editor
    {
        /// <summary>
        /// Build and Runs the current solution when clicked.
        /// </summary>
        [MenuItem("TrainAR/Build Project", false, -2005)]
        public static void BuildToDevice()
        {
            PlayModeButtonOverride.ShowDialogBoxForBuilding();
        }
    
        /// <summary>
        /// Switches the current platform to Android when clicked.
        /// </summary>
        [MenuItem("TrainAR/Switch Platform/Android", false, -2001)]
        public static void SwitchToAndroid()
        {
            PlayModeButtonOverride.SwitchBuildTargetToAndroid();
            Debug.Log("Successfully switched the platform to Android. ");
        }
    
        /// <summary>
        /// Switches the current platform to iOS when clicked.
        /// </summary>
        [MenuItem("TrainAR/Switch Platform/iOS", false, -2002)]
        public static void SwitchToIOS()
        {
            PlayModeButtonOverride.SwitchBuildTargetToIOS();
            Debug.Log("Successfully switched the platform to iOS. ");
        }
    
        //Layout Options
        
        /// <summary>
        /// Switches the Editor Layout to TrainAR Authoring mode when clicked.
        /// </summary>
        [MenuItem("TrainAR/Open TrainAR Authoring Tool", false, -1002)]
        public static void SwitchToTrainARMode()
        {
            if (EditorUtility.LoadWindowLayout(Application.dataPath + "/Editor/Layouts/TrainARLayout.wlt"))
            {
                //Loads and opens the trainAR default scene
                OpenTrainARScene();
                
                //Reset the scene to have the default camera angle and pickability/visibility restrictions
                ResetTrainARSceneToAuthoringToolDefault();
            
                //Resets the statemachine to the default statemachine
                ResetStatemachineToDefault();

                //Hides the TrainAR framework object in the hierarchy
                HideFrameworkHierarchy();

                //Force Unity into 3D mode
                SceneView.lastActiveSceneView.in2DMode = false;
                
                Debug.Log("Successfully switched to TrainAR authoring tool. ");
            }
            else
            {
                Debug.LogError("Unable to switch to TrainAR authoring tool. TrainARLayout.wlt was not found.");
            }
        }
    
        /// <summary>
        /// Switches the Editor Layout to the default Unity Layout when clicked.
        /// </summary>
        [MenuItem("TrainAR/Open Unity Editor", false, -1001)]
        public static void SwitchToUnityMode()
        {
            if (EditorUtility.LoadWindowLayout(Application.dataPath + "/Editor/Layouts/DefaultLayout.wlt"))
            {
                //Shows the TrainAR framework object in the hierarchy
                ShowFrameworkHierarchy();
                
                //Show all Objects and make all objects pickable
                SceneVisibilityManager.instance.EnableAllPicking();
                SceneVisibilityManager.instance.ShowAll();

                Debug.Log("Successfully switched to Unity Editor mode. ");
            }
            else
            {
                Debug.LogError("Unable to switch to default Unity Layout. DefaultLayout.wlt was not found.");
            }
        }

        /// <summary>
        /// Resets the scene camera to a pivot point, rotation and distance that looks good in the Scene.
        /// </summary>
        [MenuItem("TrainAR/Options/Reset TrainAR Camera + Setup", false, 999)]
        public static void ResetTrainARSceneToAuthoringToolDefault()
        {
            //Get the current sceneView, which holds the scenecamera in the unity editor
            SceneView scene = SceneView.lastActiveSceneView;
            
            //Uncomment this to get a DebugLog of the current camera internals for another default angle
            //Debug.Log("Pivot Point: " + scene.pivot + ", rotation: " + scene.rotation + ", distance: " + scene.size);
            
            //Resets the scene camera to a hardcoded value set that has the 
            scene.pivot = new Vector3(0.00f, 0.00f, 0.00f);
            scene.rotation = Quaternion.Euler(new Vector3(45, 315, 0));
            scene.size = 1f;

            //Reset the visibility and pickability of TrainAR objects
            GameObject trainARFrameWorkParent = GameObject.FindWithTag("TrainAR");
            
            //We have to set the hideflags to something different before accessing SceneVisibility
            HideFlags currentHideFlags = trainARFrameWorkParent.hideFlags;
            trainARFrameWorkParent.hideFlags = HideFlags.None;
            
            //Disable scene picking for all TrainAR framework objects
            SceneVisibilityManager.instance.DisablePicking(trainARFrameWorkParent, true);
            
            //Backup in case the first one fails...which for whatever reason happens sometimes 
            if (SceneVisibilityManager.instance.IsPickingDisabled(trainARFrameWorkParent, true))
            {
                foreach (var childTransform in trainARFrameWorkParent.GetComponentsInChildren<Transform>())
                {
                    SceneVisibilityManager.instance.DisablePicking(childTransform.gameObject, false); 
                }
            }
            
            //Make all scene objects visible again - Resetting actions from HideTrainARReferenceScene()
            SceneVisibilityManager.instance.ToggleVisibility(trainARFrameWorkParent, true);
            if (!SceneVisibilityManager.instance.AreAllDescendantsVisible(trainARFrameWorkParent))
            {
                SceneVisibilityManager.instance.ToggleVisibility(trainARFrameWorkParent, true);
            }
            
            //Disable the visibility for UI elements in the scene
            GameObject trainARUserInterface = trainARFrameWorkParent.transform.Find("UI").gameObject;
            if (SceneVisibilityManager.instance.AreAllDescendantsVisible(trainARUserInterface))
            {
                SceneVisibilityManager.instance.ToggleVisibility(trainARUserInterface, true);
            }
            
            //Resetting the hideflags to the original ones
            trainARFrameWorkParent.hideFlags = currentHideFlags;
            
            //Repaint/Update the scene camera
            scene.Repaint();
            
            Debug.Log("The TrainAR Scene was successfully reset.");
        }
        
        /// <summary>
        /// Hides the TrainAR reference setup in the scene when clicked.
        /// </summary>
        [MenuItem("TrainAR/Options/Hide TrainAR Reference Setup", false, 1001)]
        public static void HideTrainARReferenceScene()
        {
            //Reset the visibility and pickability of TrainAR objects
            GameObject trainARFrameWorkParent = GameObject.FindWithTag("TrainAR");

            //We have to set the hideflags to something different before accessing SceneVisibility
            HideFlags currentHideFlags = trainARFrameWorkParent.hideFlags;
            trainARFrameWorkParent.hideFlags = HideFlags.None;

            //Disable visibility for all TrainAR framework objects
            SceneVisibilityManager.instance.ToggleVisibility(trainARFrameWorkParent.gameObject, true);
            if (SceneVisibilityManager.instance.AreAllDescendantsVisible(trainARFrameWorkParent.gameObject))
            {
                SceneVisibilityManager.instance.ToggleVisibility(trainARFrameWorkParent.gameObject, true);
            }
            
            //Resetting the hideflags to the original ones
            trainARFrameWorkParent.hideFlags = currentHideFlags;
            
            Debug.Log("The TrainAR reference setup is now hidden in the scene.");
        }
        
        /// <summary>
        /// Opens the default TrainAR scene when clicked.
        /// </summary>
        [MenuItem("TrainAR/Options/Open TrainAR Scene", false, 2000)]
        public static void OpenTrainARScene()
        {
            //Save the current scene if it is a TrainAR scene
            if (GameObject.FindWithTag("TrainAR") != null)
            {
                EditorSceneManager.SaveOpenScenes();
            }
            EditorSceneManager.OpenScene("Assets/Scene.unity");
            Debug.Log("The TrainAR default Scene was successfully loaded.");
        }
        
        /// <summary>
        /// Opens the default statemachine in the visual scripting window when clicked.
        /// </summary>
        [MenuItem("TrainAR/Options/Open TrainAR Statemachine", false, 2001)]
        public static void ResetStatemachineToDefault()
        {
            var objectReferenceToStatemachine = AssetDatabase.LoadMainAssetAtPath("Assets/Statemachine.asset");
            
            if (AssetDatabase.OpenAsset(objectReferenceToStatemachine))
            {
                Debug.Log("The TrainAR default Statemachine was successfully loaded.");
            }
            else
            {
                Debug.LogError("The TrainAR Statemachine could not be found. Make sure the \"Statemachone.asset\" file is located in the projects root folder. ");
            }
        }
        
        /// <summary>
        /// Hides the framework GameObject in the hierarchy when clicked.
        /// </summary>
        [MenuItem("TrainAR/Options/Hide TrainAR Framework in Hierarchy", false, 3000)]
        public static void HideFrameworkHierarchy()
        {
            var trainARFramework = GameObject.FindGameObjectWithTag("TrainAR");
            if (trainARFramework != null)
            {
                trainARFramework.hideFlags = HideFlags.HideInHierarchy;
                Debug.Log("Hiding the TrainAR framework object in the hierarchy. ");
            }
            else
            {
                Debug.LogError("Could not find the TrainAR framework object. Are you sure it still exists and is active?");
            }
        }
        
        /// <summary>
        /// Shows the framework GameObject in the hierarchy when clicked.
        /// </summary>
        [MenuItem("TrainAR/Options/Show TrainAR Framework in Hierarchy", false, 3001)]
        public static void ShowFrameworkHierarchy()
        {
            var trainARFramework = GameObject.FindGameObjectWithTag("TrainAR");
            if (trainARFramework != null)
            {
                trainARFramework.hideFlags = HideFlags.None;
                Debug.Log("Showing the TrainAR framework object in the hierarchy again.");
            }
            else
            {
                Debug.LogError("Could not find the TrainAR framework object. Are you sure it still exists and is active?");
            }
        }
        
        /// <summary>
        /// Opens the Documentation when clicked.
        /// </summary>
        [MenuItem("TrainAR/Open Documentation", false, 4000)]
        public static void OpenDocumentation()
        {
            Help.BrowseURL("https://jblattgerste.github.io/TrainAR/index.html");
        }
    }
}
