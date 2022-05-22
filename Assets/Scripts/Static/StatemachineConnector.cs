using System;
using System.Collections.Generic;
using UnityEngine;
using UI;
using static UI.QuestionnaireController;

namespace Static
{
    /// <summary>
    /// The type of interaction that is used in a StateInformation struct through a state change request.
    /// </summary>
    public enum InteractionType
    {
        Select, // Not used in the statemachine but implemented and has Event on the TrainARObject
        Deselect, // Not used in the statemachine but implemented and has Event on the TrainARObject
        Grab,
        Release, // Not used in the statemachine but implemented and has Event on the TrainARObject
        Interact,
        Combine,
        Custom
    }

    /// <summary>
    /// The StateInformation is a struct created for state change requests. It holds the information of the request to the statemachine.
    /// </summary>
    public struct StateInformation
    {
        /// <summary>
        /// The primary object of this request.
        /// </summary>
        public string primaryObjectName;
        /// <summary>
        /// The secondary object of this request (e.g. for a combination, the object that is not grabbed)
        /// </summary>
        public string secondaryObjectName; //only used for Combining objects
        /// <summary>
        /// The type of interaction that is requested.
        /// </summary>
        public InteractionType interactionType;
        /// <summary>
        /// The parameter that is passed with the request. E.g. for custom actions or questionnaires this can also be the only thing that is checked
        /// against by the statemachine.
        /// </summary>
        public string parameter;
        /// <summary>
        /// The primary GameObject request of this request.
        /// </summary>
        public GameObject firstGameObject;
        /// <summary>
        /// The secondary GameObject request of this request.
        /// </summary>
        public GameObject secondGameObject;

        /// <summary>
        /// Constructor of the StateInformation struct.
        /// </summary>
        public StateInformation(string primaryObjectName = "", string secondaryObjectName = "", InteractionType interactionType = InteractionType.Custom,
            string parameter = " ", GameObject firstGameObject = null, GameObject secondGameObject = null)
        {
            this.primaryObjectName = primaryObjectName;
            this.secondaryObjectName = secondaryObjectName;
            this.interactionType = interactionType;
            this.parameter = parameter;
            this.firstGameObject = firstGameObject;
            this.secondGameObject = secondGameObject;
        }
    }

    /// <summary>
    /// The StatemachineConnector connects interaction inputs with the visual scripting flow and outputs 
    /// with the instruction, error and insight controller through events. It therefore functions as a funnel
    /// between the Visual Statemachine and the rest of the logic.
    ///
    /// Either the visual statemachien decides whether actions are correct or wrong (default) or the connection
    /// can also be commented out and the decisions on wrong/correct actions can be made manually in the
    /// RequestStateChange method by using C# programming.
    /// </summary>
    public class StatemachineConnector
    {
        /// <summary>
        /// The static instance of this StatemachineConnector.
        /// </summary>
        public static StatemachineConnector Instance = new StatemachineConnector();
        
        /// <summary>
        /// The amount of errors performed by the user of the training.
        /// </summary>
        public int errorCounter = 0;

        public event Action<string, int> TriggerTopPanelChange;
        public event Action<bool> TriggerAcceptedStateChange;
        public event Action<string, string> TriggerErrorOverlay;
        public event Action<AudioClip, Sprite, string> TriggerExpertInsights;
        public event Action TriggerScenarioCompletionOverlay;
        public event Action<QuestionUITypes, string, List<Answer>> TriggerUIQuestion;
 
        private static List<StateInformation> stateHistory = new List<StateInformation>();
        private bool acceptedStateChange;
        private static Func<StateInformation, bool> stateChangeTrigger;
        private StatemachineConnector() {}
        
        /// <summary>
        /// Registers a new state change trigger from the visual state flow. This trigger
        /// determines which node in the visual scripting graph is handling the state change request.
        /// </summary>
        /// <param name="action">The Func to that is registered.</param>
        public static void RegisterNewStateChangeTrigger(Func<StateInformation, bool> action)
        {
            stateChangeTrigger = null;
            stateChangeTrigger = action;
        }
        
        /// <summary>
        /// Request a statechange from the statemachine.
        /// </summary>
        /// <param name="stateInformation"></param>
        /// <returns></returns>
        public bool RequestStateChange(StateInformation stateInformation)
        {
            //Each Combine triggers on both objects so the second pass doesn't need to be checked in the catalog again or added to the history
            /*if ((stateHistory.Count > 1) &&
                (stateHistory[stateHistory.Count - 1].interactionType == InteractionType.Combine) &&
                (stateInformation.interactionType == InteractionType.Combine) &&
                (stateHistory[stateHistory.Count - 1].secondaryObjectName == stateInformation.primaryObjectName) &&
                (stateHistory[stateHistory.Count - 1].primaryObjectName == stateInformation.secondaryObjectName))
            {
                //this returns the result of the first combine check
                return acceptedStateChange;
            }*/
            
            //Add the requested statechange to the state history
            stateHistory.Add(stateInformation);

            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            /// ReSharper disable once InvalidXmlDocComment
            ///
            /// Visual Statemachine Connection!
            ///
            /// If you want to use the TrainAR framework without the visual scripting components, you can comment this
            /// part out and handle the StateRequestChanges manually here and decide if this was an "acceptedStateChange"
            /// or not which is then in turn used on the TrainARObject Events to trigger potential actions.
            ///
            /// In this case you also have to handle outputs manually. This is possible by just manually calling the methods
            /// UpdateTopPanel(), ShowErrorOverlay(), ShowExpertInsights(), StartQuestionUI(), and ShowCompletionOverlay()
            /// in this script to trigger the corresponding funtions of the TrainAR framework.
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //Check if this requested state change is accepted by consulting the visuals statemachine flow
            acceptedStateChange = stateInformation.interactionType switch
            {
                InteractionType.Grab => true, //For grabbing this is always true, selection, deselection and release are not calling this
                InteractionType.Combine => stateChangeTrigger.Invoke(stateInformation),
                InteractionType.Interact => stateChangeTrigger.Invoke(stateInformation),
                InteractionType.Custom => stateChangeTrigger.Invoke(stateInformation),
                _ => acceptedStateChange
            };
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////

            //Print the state change to the debug console
            PrintStateChangeRequest(stateHistory[^1], acceptedStateChange);
            if(stateInformation.interactionType != InteractionType.Grab) AcceptedStateChange(acceptedStateChange);
            //If this was an incorrect action, increment the error counter
            if (!acceptedStateChange)
            {
                errorCounter++;
            }

            //Return if this was an accepted statechange to the TrainAR object to proceed to trigger actions on itself
            return acceptedStateChange;
        }

        /// <summary>
        /// Print the requested Stage to the log.
        /// </summary>
        /// <param name="stateInformation"></param>
        /// <param name="correct"></param>
        private void PrintStateChangeRequest(StateInformation stateInformation, bool correct)
        {
            string correctString = correct ? "CORRECT" : "WRONG";
            switch (stateInformation.interactionType)
            {
                case InteractionType.Combine:
                    Debug.Log("Statemachine: Trying to COMBINE " + stateInformation.primaryObjectName + " and " +
                              stateInformation.secondaryObjectName + ". This was: " + correctString);
                    break;
                case InteractionType.Select: //Not used in the current implementation
                    Debug.Log("Statemachine: Trying to SELECT " + stateInformation.primaryObjectName + ". This was: " +
                              correctString);
                    break;
                case InteractionType.Deselect: //Not used in the current implementation
                    Debug.Log("Statemachine: Trying to DESELECT " + stateInformation.primaryObjectName + ". This was: " +
                              correctString);
                    break;
                case InteractionType.Grab: //Used in the current implementation but always returns true in RequestStateChange
                    Debug.Log("Statemachine: Trying to GRAB " + stateInformation.primaryObjectName + ". This was: " +
                              correctString);
                    break;
                case InteractionType.Release: //Not used in the current implementation
                    Debug.Log("Statemachine: Trying to RELEASE " + stateInformation.primaryObjectName + ". This was: " +
                              correctString);
                    break;
                case InteractionType.Interact:
                    Debug.Log("Statemachine: Trying to INTERACT WITH " + stateInformation.primaryObjectName +
                              ". This was: " + correctString);
                    break;
                case InteractionType.Custom:
                    Debug.Log("Statemachine: Trying to do a CUSTOM ACTION with the parameter " + stateInformation.parameter+ ". This was: " + correctString);
                    break;
                default:
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Called by the visual state machine, triggers an Action that updates the instruction text and completion percentage
        /// of the top panel.
        /// </summary>
        /// <param name="text">New instruction displayed on the top panel</param>
        /// <param name="completionPercentage">New compeltion percentage displayed</param>
        public void UpdateTopPanel(string text, int completionPercentage)
        {
            Debug.Log("Statemachine: A new Instruction is displayed on the top panel: \"" + text + "\". The scenario is " + completionPercentage.ToString() + "% completed.");
            TriggerTopPanelChange?.Invoke(text, completionPercentage);
        }

        /// <summary>
        /// Called by the state machine. Triggers an Action that indicates if the state change was accepted.
        /// </summary>
        /// <param name="stateChangeAccepted">Is the statechange accepted</param>
        public void AcceptedStateChange(bool stateChangeAccepted)
        {
            Debug.Log("Statemachine: A new state change was requested: The result of the state change was " + stateChangeAccepted.ToString());
            TriggerAcceptedStateChange?.Invoke(stateChangeAccepted);
        }

        /// <summary>
        /// Called by the visual state machine. Triggers an Action that shows the error overlay.
        /// </summary>
        /// <param name="headerText">The Header of the error overlay</param>
        /// <param name="errorText">The text body of the error overlay</param>
        public void ShowErrorOverlay(string headerText, string errorText)
        {
            Debug.Log("Statemachine: The Error Overlay \"" + headerText + "\" was triggered with the following text: \"" + errorText + "\".");
            TriggerErrorOverlay?.Invoke(headerText, errorText);
        }

        /// <summary>
        /// Called by the visual state machine. Triggers an Action that shows the expert insight modality and plays and audio clip.
        /// </summary>
        /// <param name="audio">The audioclip that is played with the expert insight</param>
        /// <param name="image">The image that is displayed, e.g. the expert providing the insight.</param>
        /// <param name="text">The text displayed at the expert speechbubble</param>
        public void ShowExpertInsights(AudioClip audio, Sprite image, string text)
        {
            Debug.Log("Statemachine: The Expert Insights were triggered and displayed: \"" + text + "\". Audio was played: " + (audio == null ? "no" : "yes") + ".");
            TriggerExpertInsights?.Invoke(audio, image, text);
        }

        /// <summary>
        /// Triggers the scenario completion overlay.
        /// </summary>
        public void ShowCompletionOverlay()
        {
            Debug.Log("Statemachine: The scenario completion overlay was triggered and the summary displayed.");
            TriggerScenarioCompletionOverlay?.Invoke();
        }

        /// <summary>
        /// Triggers a new Questionnaire to be answered by the user on the UI.
        /// </summary>
        /// <param name="type">The type of Question(MultipleChoice, List, Input).</param>
        /// <param name="question">The question.</param>
        /// <param name="answers">A List of struct with all possible answers.</param>

        public void StartQuestionUI(QuestionUITypes type, string question, List<Answer> answers)
        {
            Debug.Log("Statemachine: The TrainAR Question \"" + question + "\" was triggered. This is a " + type);
            TriggerUIQuestion?.Invoke(type, question, answers);
        }

        /// <summary>
        /// Resets the instance of this static instance.
        /// </summary>
        public void Reset()
        {
            Instance = new StatemachineConnector();
        }
    }
}
