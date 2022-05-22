using Static;
using UI;
using Unity.VisualScripting;
using UnityEngine;

namespace Visual_Scripting
{
    /// <summary>
    /// Implements the "Instruction" functionality of the TrainAR Framework, therefore it updates the isntrutions and progress percentage of the Top panel.
    /// </summary>
    [UnitTitle("TrainAR: Instructions")] //The title of the unit
    [UnitSubtitle("Updates the instructions and progress\npercentage in the top UI panel")] //Explanation of the unit
    [UnitCategory("TrainAR")] //The folder it is stored at in the flow graph unit menu
    [TypeIcon(typeof(UnityEngine.Device.Application))]
    public class Instructions : Unit
    {
        /// <summary>
        /// The Input port of the Unit that triggers the internal logic.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ControlInput InputFlow { get; private set; }

        /// <summary>
        /// The Output port of the Unit that is tirggered after executing the units logic.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ControlOutput OutputFlow { get; private set; }

        /// <summary>
        /// The instruction text that is displayed for the current step.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueInput instructionText { get; private set; }

        /// <summary>
        /// The percentage that is displayed for the current step.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueInput progressPercentage { get; private set; }

        /// <summary>
        /// Defines the Nodes input, output and value ports.
        /// </summary>
        protected override void Definition()
        {
            
            //Defining the Input port of the flow & Node Logic
            InputFlow = ControlInput("", NodeLogic);
            
            //Output flow (this is only a paththrough for an output Unit)
            OutputFlow = ControlOutput("");

            //ValueInputs of the unit
            instructionText = ValueInput<string>("Instructions", string.Empty);
            progressPercentage = ValueInput<int>("Progress percentage", 0);
        }
    
        /// <summary>
        /// The NodeLogic that is triggered when an input flow is detected on the controlInput.
        ///
        /// This triggers the top panel to display a new instruction text and updates the progress circle.
        /// </summary>
        /// <param name="flow">The current flow of the graph</param>
        /// <returns>Returns to the output flow immediatly after triggering its internal logic</returns>
        private ControlOutput NodeLogic(Flow flow)
        {
            //Updates the top panel with a new instruction text and new completion percentage value
            StatemachineConnector.Instance.UpdateTopPanel(flow.GetValue<string>(instructionText), flow.GetValue<int>(progressPercentage));

            //Return the outputflow, therefore instantly after triggering its logic continues the graph
            return OutputFlow;
        }
    }
}
