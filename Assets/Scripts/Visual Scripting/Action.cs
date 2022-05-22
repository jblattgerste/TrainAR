using System;
using Static;
using Unity.VisualScripting;
using UnityEngine;

namespace Visual_Scripting
{
    /// <summary>
    /// Implements the "Interacting", "Combining" and "Custom" Action functionality of the TrainAR Framework,
    /// therefore it checks if the  user triggered the "Interact", "Combine" Buttons or triggered a CustomAction through
    /// scripts and allows to check this against the stored state in the visual statemachine.
    /// </summary>
    [UnitTitle("TrainAR: Action")] //The title of the Unity
    [UnitSubtitle("Waits for the user to perform a single Action\n(Interaction, Combination, or CustomAction) and\ntriggers the correct or incorrect stateflow output")] //The unit group this belongs to 
    [UnitCategory("TrainAR")] //The folder it is stored at in the flow graph unit menu
    [TypeIcon(typeof(This))]
    public class Action : Unit
    {
    
        /// <summary>
        /// The type of interaction to check against.
        /// </summary>
        public enum TrainARActionChoices
        {
            Interaction,
            Combination,
            CustomAction
        }
        /// <summary>
        /// What type of actions are accepted by this node.
        /// </summary>
        /// <value>Set in node in the editor. Default is Interaction.</value>
        [UnitHeaderInspectable("Action: ")]
        public TrainARActionChoices actionChoice = TrainARActionChoices.Interaction;
        /// <summary>
        /// The Input port of the Unit that triggers the internal logic.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ControlInput InputFlow { get; private set; }

        /// <summary>
        /// The Output port of the Unity that is triggered when the users interaction was CORRECT.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ControlOutput CorrectAction { get; private set; }

        /// <summary>
        /// The Output port of the Unity that is triggered when the users interaction was INCORRECT.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ControlOutput IncorrectAction { get; private set; }

        /// <summary>
        /// The Name of the first correct interactable for this step.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueInput ARCombinableName1 { get; private set; }

        /// <summary>
        /// The Name of the second correct interactable for this step.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueInput ARCombinableName2 { get; private set; }

        /// <summary>
        /// The Graphreference stores the current position in the flow graph to revisit it on Event/Action triggers.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public GraphReference graphReference { get; private set; }

        /// <summary>
        /// The stored name of the interactables to check them against the interaction.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        private string combinableName1;
        /// <summary>
        /// The stored name of the interactables to check them against the interaction.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        private string combinableName2;

        /// <summary>
        /// The Name of the correct interactable for this step.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueInput ARInteractableName { get; private set; }

        /// <summary>
        /// The stored name of the interactables to check them against the interaction.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        private string interactableName;

        /// <summary>
        /// The correct parameter for the custom event.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueInput CorrectParameterText { get; private set; }

        /// <summary>
        /// The correct parameter handed by the custom event.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        private string customActionParameter;

        /// <summary>
        /// Defines the Nodes input, output and value ports.
        /// </summary>
        protected override void Definition()
        {
            //Defining the Input port of the flow & Node Logic
            InputFlow = ControlInput("", NodeLogic);
        
            //Static Output flow that is triggered dependend on the NodeLogic
            CorrectAction = ControlOutput("Correct");
            IncorrectAction = ControlOutput("Incorrect");

            //Define ValueInputs and ValueOutputs based on the selected action to perform
            switch (actionChoice)
            {
                case TrainARActionChoices.Interaction:
                    ARInteractableName = ValueInput<string>("Correct object", string.Empty);
                    break;
                case TrainARActionChoices.Combination:
                    ARCombinableName1 = ValueInput<string>("Correct grabbed object", string.Empty);
                    ARCombinableName2 = ValueInput<string>("Correct stationary object", string.Empty);
                    break;
                case TrainARActionChoices.CustomAction:
                    CorrectParameterText = ValueInput<string>("Correct parameter", string.Empty);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    
        /// <summary>
        /// The NodeLogic that is triggered when an input flow is detected on the controlInput.
        ///
        /// This sets the instruction text at the UI element at the top of the smartphone and registers an Action
        /// with StateInformation that can be triggered the TrainAR StateChecker.
        /// </summary>
        /// <param name="flow">The current flow of the graph</param>
        /// <returns>Null, as the flow is stopped until the registered event is triggered.</returns>
        private ControlOutput NodeLogic(Flow flow)
        {
            switch (actionChoice)
            {
                case TrainARActionChoices.Interaction:
                    interactableName = flow.GetValue<string>(ARInteractableName);
                    break;
                case TrainARActionChoices.Combination:
                    combinableName1 = flow.GetValue<string>(ARCombinableName1);
                    combinableName2 = flow.GetValue<string>(ARCombinableName2);
                    break;
                case TrainARActionChoices.CustomAction:
                    customActionParameter = flow.GetValue<string>(CorrectParameterText);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //Store the current position in the graph as we have to revisit it later
            graphReference = flow.stack.AsReference();

            //Register an Action and pass it to the TrainAR StateChecker to trigger it when ready
            Func<StateInformation, bool> triggerEvent = ContinueFlow;
            StatemachineConnector.RegisterNewStateChangeTrigger(triggerEvent);
        
            //Return null instead of ControLOutput to pause the graph flow until it is revisited by ContinueFlow
            return null;
        }
        
        /// <summary>
        /// Continues the flow when the corresponding action registered in the NodeLogic is triggered.
        /// </summary>
        /// <param name="stateInformation">The Information of the requested Statechange</param>
        private bool ContinueFlow(StateInformation stateInformation)
        {
            switch (actionChoice)
            {
                case TrainARActionChoices.Interaction:
                    //Checks if the user tries the correct action
                    if (stateInformation.interactionType == InteractionType.Interact)
                    {
                        //Checks if the user tries this (correct) interaction with the correct object
                        if (stateInformation.primaryObjectName == interactableName)
                        {
                            Flow.New(graphReference).Invoke(CorrectAction);
                            return true;
                        }
                        else
                        {
                            Flow.New(graphReference).Invoke(IncorrectAction); //Wrong object
                            return false;
                        }
                    }
                    else
                    {
                        Flow.New(graphReference).Invoke(IncorrectAction); //wrong action
                        return false;
                    }
                case TrainARActionChoices.Combination:
                    //Checks if the user tries the correct action
                    if (stateInformation.interactionType == InteractionType.Combine)
                    {
                        //Checks if the user tries this (correct) combination with the correct objects
                        if ((stateInformation.primaryObjectName == combinableName1 && stateInformation.secondaryObjectName == combinableName2))
                        {
                            Flow.New(graphReference).Invoke(CorrectAction);
                            return true;
                        }
                        else
                        {
                            Flow.New(graphReference).Invoke(IncorrectAction); //Wrong object
                            return false;
                        }
                    }
                    else
                    {
                        Flow.New(graphReference).Invoke(IncorrectAction); //wrong action
                        return false;
                    }
                case TrainARActionChoices.CustomAction:
                    //Checks if the user tries the correct action
                    if (stateInformation.interactionType == InteractionType.Custom)
                    {
                        //Checks if the user tries this (correct) interaction with the correct value
                        if (stateInformation.parameter == customActionParameter)
                        {
                            Flow.New(graphReference).Invoke(CorrectAction);
                            return true;
                        }
                        else
                        {
                            Flow.New(graphReference).Invoke(IncorrectAction); //Wrong object
                            return false;
                        }
                    }
                    else
                    {
                        Flow.New(graphReference).Invoke(IncorrectAction); //wrong action
                        return false;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
