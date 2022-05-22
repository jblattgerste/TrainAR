using UnityEditor;
using UnityEngine;
using Others;

namespace Editor.Scripts
{
    /// <summary>
    /// Adds the GUI to the inspector to simulate stateRequests to test the statemachine in the editor.
    /// This is an Unity Editor Utility Class for the "StateRequestSimulator" class.
    /// </summary>
    [CustomEditor(typeof(StateRequestSimulator))]
    public class StateRequestSimulatorEditor : UnityEditor.Editor
    {
        /// <summary>
        /// Adds the inspector GUI.
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            StateRequestSimulator simulator = (StateRequestSimulator)target;
            if (GUILayout.Button("Simulate Request State Change"))
            {
                simulator.SimulateStateChangeRequest();
            }
        }
    }
}
