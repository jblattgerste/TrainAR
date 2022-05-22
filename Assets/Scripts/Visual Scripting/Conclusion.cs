using Static;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Playables;

namespace Visual_Scripting
{
    /// <summary>
    /// Triggers the Completion overlay that shows the training assessment of the scenario and ends the stateflow/training.
    /// </summary>
    [UnitTitle("TrainAR: Training Conclusion")] //The title of the unit
    [UnitSubtitle("Triggers the completion overlay, shows \nfeedback and concludes the training")] //Explanation of the unit
    [UnitCategory("TrainAR")] //The folder it is stored at in the flow graph unit menu
    [TypeIcon(typeof(UnityWebRequest.Result))]
    public class Conclusion : Unit
    {
        /// <summary>
        /// The Input port of the Unit that triggers the internal logic.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ControlInput InputFlow { get; private set; }

        /// <summary>
        /// Defines the Nodes input and value ports
        ///
        /// As this terminates the scenario, there is no output port or flow, the rest is handled through the
        /// script-based components in the framework.
        /// </summary>
        protected override void Definition()
        {
            //Defining the Input port of the flow & Node Logic
            InputFlow = ControlInput("End", NodeLogic);
        }
    
        /// <summary>
        /// The NodeLogic that is triggered when an input flow is detected on the controlInput.
        ///
        /// This triggers the completion overlay and concludes the scenario and flow graph
        /// </summary>
        /// <param name="flow">The current flow of the graph</param>
        /// <returns>Returns to the output flow immediatly after triggering its internal logic</returns>
        private ControlOutput NodeLogic(Flow flow)
        {
            StatemachineConnector.Instance.ShowCompletionOverlay();
            
            //Returns null as the graph terminates here
            return null;
        }
    }
}
