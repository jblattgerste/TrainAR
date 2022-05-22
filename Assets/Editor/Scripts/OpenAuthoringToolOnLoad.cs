using UnityEditor;
using UnityEngine;

namespace Editor.Scripts
{
    /// <summary>
    /// Every time the editor is started, trigger switching to the TrainAR authoring tool.
    /// </summary>
    [InitializeOnLoad]
    public class OpenAuthoringToolOnLoad
    {
        private static bool _initialReloadWasCompleted = false;
        
        /// <summary>
        /// On initialization register an even that listens to the editor inspectors being repainted.
        /// </summary>
        static OpenAuthoringToolOnLoad()
        {
            //Deprecated: This sometimes causes problems on being reset when scripts are recompiled
            //EditorApplication.delayCall += OnInspectorsWereReloaded;
        }

        /// <summary>
        /// Only the very first time this happens since unity editor was started, trigger opening the TrainAR authoring tool.
        /// </summary>
        private static void OnInspectorsWereReloaded()
        {
            //Return if the automatic loading was already completed
            if (_initialReloadWasCompleted) return;
            _initialReloadWasCompleted = true;
            
            //Trigger switching to the TrainAR authoring tool
            TrainAREditorMenu.SwitchToTrainARMode();
            Debug.Log("Automatic TrainAR authoring tool loading successfully loaded.");
        }
    }
}
