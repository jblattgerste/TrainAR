using Static;
using Unity.VisualScripting;

namespace Visual_Scripting
{
    /// <summary>
    /// Implements the "Feedback" functionality of the TrainAR Framework, therefore it triggers
    /// the error overlay modality to show feedback (e.g. for an incorrect important action).
    /// </summary>
    [UnitTitle("TrainAR: Feedback")] //The title of the unit
    [UnitSubtitle("Triggers the error overlay (e.g. for\nespecially important textual feedback)")] //Explanation of the unit
    [UnitCategory("TrainAR")] //The folder it is stored at in the flow graph unit menu
    [TypeIcon(typeof(System.Exception))]
    public class Feedback : Unit
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
        /// The Header text that is displayed for the current step.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueInput HeaderText { get; private set; }

        /// <summary>
        /// The Error text that is displayed for the current step.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueInput ErrorText { get; private set; }

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
            HeaderText = ValueInput<string>("Header text", string.Empty);
            ErrorText = ValueInput<string>("Feedback text", string.Empty);
        }
    
        /// <summary>
        /// The NodeLogic that is triggered when an input flow is detected on the controlInput.
        ///
        /// This triggers the error overlay and displays the inserted error text until it is disposed by the user
        /// </summary>
        /// <param name="flow">The current flow of the graph</param>
        /// <returns>Returns to the output flow immediatly after triggering its internal logic</returns>
        private ControlOutput NodeLogic(Flow flow)
        {
            StatemachineConnector.Instance.ShowErrorOverlay(flow.GetValue<string>(HeaderText),flow.GetValue<string>(ErrorText));
            
            //Return the outputflow, therefore instantly after triggering its logic continues the graph
            return OutputFlow;
        }
    }
}
