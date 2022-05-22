using System;
using Static;
using UI;
using Unity.VisualScripting;
using UnityEngine;

namespace Visual_Scripting
{
    /// <summary>
    /// Implements the "Insights" functionality of the TrainAR Framework, therefore it triggers
    /// the expert input modality to show additional insights and tips and optionally plays audio clips.
    /// </summary>
    [UnitTitle("TrainAR: Insights")] //The title of the unit
    [UnitSubtitle("Triggers the expert insight element under\nthe top panel to display additional insights")] //Explanation of the unit
    [UnitCategory("TrainAR")] //The folder it is stored at in the flow graph unit menu
    [TypeIcon(typeof(AnimatorStateInfo))]
    public class Insights : Unit
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
        public ValueInput InsightText { get; private set; }

        /// <summary>
        /// The audioclip to be played with the expert insights.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueInput InsightAudioClip { get; private set; }

        /// <summary>
        /// The image/symbol to be displayed as the "expert" next to the text.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueInput InsightExpertImage { get; private set; }

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
            InsightText = ValueInput<string>("Insights", string.Empty);
            InsightAudioClip = ValueInput<AudioClip>("Audio clip", null);
            InsightExpertImage = ValueInput<Sprite>("Image", null);
        }
    
        /// <summary>
        /// The NodeLogic that is triggered when an input flow is detected on the controlInput.
        ///
        /// This triggers the expert Insights that are displayed under the UI elements
        /// </summary>
        /// <param name="flow">The current flow of the graph</param>
        /// <returns>Returns to the output flow immediatly after triggering its internal logic</returns>
        private ControlOutput NodeLogic(Flow flow)
        {
            StatemachineConnector.Instance.ShowExpertInsights(
                InsightAudioClip == null ? null : flow.GetValue<AudioClip>(InsightAudioClip),
                InsightExpertImage == null ? null : flow.GetValue<Sprite>(InsightExpertImage),
                flow.GetValue<string>(InsightText));

            //Return the outputflow, therefore instantly after triggering its logic continues the graph
            return OutputFlow;
        }
    }
}
