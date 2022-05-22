using System;
using System.Collections.Generic;
using Static;
using Unity.VisualScripting;
using UnityEngine;
using UI;
using static UI.QuestionnaireController;

namespace Visual_Scripting
{
    /// <summary>
    /// Implements the UI questionnaire functionality of TrainAR, therefore it triggers and checks against UI questions like
    /// text input fields, questionnaires, and question lists.
    /// </summary>
    [UnitTitle("TrainAR: Action (UI)")] //The title of the Unity
    [UnitSubtitle("Waits for the user to complete a UI-based text input,\nquestionnaire or list selection task that is displayed\nby the framework automatically")] //The unit group this belongs to 
    [UnitCategory("TrainAR")] //The folder it is stored at in the flow graph unit menu
    [TypeIcon(typeof(GUI))]
    public class ActionUI : Unit
    {
        /// <summary>
        /// The type of UI questionnaire triggered and check against.
        /// </summary>
        public enum TrainARUIActionChoices
        {
            InputField,
            Questionnaire,
            ListSelection
        }
        /// <summary>
        /// What type of question is triggered by this node.
        /// </summary>
        /// <value>Set in node in the editor</value>
        [UnitHeaderInspectable("UI Task: ")]
        public TrainARUIActionChoices actionChoice = TrainARUIActionChoices.InputField;
        /// <summary>
        /// The amount of correct answers.
        /// </summary>
        /// <value>Set in node in the editor. Default is 2.</value>
        [UnitHeaderInspectable("# of Correct Answers: ")]
        public int correctAnswers = 2;
        /// <summary>
        /// The amount of wrong answers.
        /// </summary>
        /// <value>Set in node in the editor. Default is 2.</value>
        [UnitHeaderInspectable("# of Wrong Answers: ")]
        public int wrongAnswers = 2;

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
        public List<ControlOutput> CorrectAction { get; private set; } = new List<ControlOutput>();

        /// <summary>
        /// The Output port of the Unity that is triggered when the users interaction was INCORRECT.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public List<ControlOutput> IncorrectAction { get; private set; } = new List<ControlOutput>();

        /// <summary>
        /// The Text for the question for this step.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public ValueInput Question { get; private set; }

        /// <summary>
        /// The Texts for the correct answers for this step.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public List<ValueInput> CorrectAnswers { get; private set; } = new List<ValueInput>();

        /// <summary>
        /// The Texts for the correct answers for this step.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public List<ValueInput> CorrectAnswersFeedback { get; private set; } = new List<ValueInput>();

        /// <summary>
        /// The Texts for the wrong answers for this step.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public List<ValueInput> WrongAnswers { get; private set; } = new List<ValueInput>();

        /// <summary>
        /// The Texts for the wrong answers for this step.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public List<ValueInput> WrongAnswersFeedback { get; private set; } = new List<ValueInput>();

        /// <summary>
        /// The Graphreference stores the current position in the flow graph to revisit it on Event/Action triggers.
        /// </summary>
        /// <value>Set in node in the editor.</value>
        [DoNotSerialize]
        public GraphReference graphReference { get; private set; }

        private List<string> correctAnswersToCheck = new List<string>();
        private List<string> incorrectAnswersToCheck = new List<string>();

        /// <summary>
        /// Defines the Nodes input, output and value ports
        /// </summary>
        protected override void Definition()
        {
            //Defining the Input port of the flow & Node Logic
            InputFlow = ControlInput("", NodeLogic);
            Question = ValueInput<string>("Question", string.Empty);

            //Define ValueInputs and ValueOutputs based on the selected action to perform
            switch (actionChoice)
            {
                case TrainARUIActionChoices.InputField:
                    CorrectAction.Add(ControlOutput("Correct"));
                    wrongAnswers = 0;
                    if (correctAnswers <= 0) correctAnswers = 1;
                    for (int i = 0; i < correctAnswers; i++)
                    {
                        //Output flow (this is only a paththrough for an output Unit)
                        CorrectAnswers.Add(ValueInput<string>(NumberToString(i + 1) + " Correct answer", string.Empty));
                    }
                    //Static Output flow that is triggered dependend on the NodeLogic
                    IncorrectAction.Add(ControlOutput("Incorrect"));
                    break;
                case TrainARUIActionChoices.Questionnaire:
                    if (correctAnswers + wrongAnswers <= 0) correctAnswers = 1;
                    if (correctAnswers > 4) correctAnswers = 4;
                    if (correctAnswers + wrongAnswers > 4) wrongAnswers = 4 - correctAnswers;

                    for (int i = 0; i < correctAnswers; i++)
                    {
                        CorrectAction.Add(ControlOutput(NumberToString(i + 1) + " Correct"));
                        //Output flow (this is only a paththrough for an output Unit)
                        CorrectAnswers.Add(ValueInput<string>(NumberToString(i + 1) + " Correct answer", string.Empty));
                        CorrectAnswersFeedback.Add(ValueInput<string>(NumberToString(i + 1) + " Correct answer feedback", string.Empty));
                    }
                    for (int i = 0; i < wrongAnswers; i++)
                    {
                        IncorrectAction.Add(ControlOutput(NumberToString(i + 1) + " Incorrect"));
                        //Output flow (this is only a paththrough for an output Unit)
                        WrongAnswers.Add(ValueInput<string>(NumberToString(i + 1) + " Wrong answer", string.Empty));
                        WrongAnswersFeedback.Add(ValueInput<string>(NumberToString(i + 1) + " Wrong answer feedback", string.Empty));
                    }
                    //Static Output flow that is triggered dependend on the NodeLogic
                    //if(wrongAnswers>0) IncorrectAction = ControlOutput("Incorrect");
                    //Is there a "wrong" output callback here? We are only here if we got the correct one, or do we not let people repeat?
                    //TODO
                    break;
                case TrainARUIActionChoices.ListSelection:
                    CorrectAction.Add(ControlOutput("Correct"));
                    IncorrectAction.Add(ControlOutput("Incorrect"));
                    if (correctAnswers <= 0) correctAnswers = 1;
                    for (int i = 0; i < correctAnswers; i++)
                    {
                        //Output flow (this is only a paththrough for an output Unit)
                        CorrectAnswers.Add(ValueInput<string>(NumberToString(i + 1) + " Correct answer", string.Empty));
                    }
                    for (int i = 0; i < wrongAnswers; i++)
                    {
                        //Output flow (this is only a paththrough for an output Unit)
                        WrongAnswers.Add(ValueInput<string>(NumberToString(i + 1) + " Wrong answer", string.Empty));
                        WrongAnswersFeedback.Add(ValueInput<string>(NumberToString(i + 1) + " Wrong answer feedback", string.Empty));
                    }
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
            List<Answer> answers = new List<Answer>();
            switch (actionChoice)
            {
                case TrainARUIActionChoices.InputField:
                    for (int i = 0; i < correctAnswers; i++)
                    {
                        //Output flow (this is only a paththrough for an output Unit)
                        answers.Add(new Answer(flow.GetValue<string>(CorrectAnswers[i]), " ", true));
                        correctAnswersToCheck.Add(flow.GetValue<string>(CorrectAnswers[i]));
                    }
                    StatemachineConnector.Instance.StartQuestionUI(QuestionUITypes.InputQuestion, flow.GetValue<string>(Question), answers);
                    //GameObject.FindObjectOfType<QuestionnaireController>().InitInputfield(flow.GetValue<string>(Question));
                    break;
                case TrainARUIActionChoices.Questionnaire:
                    for (int i = 0; i < correctAnswers; i++)
                    {
                        //Output flow (this is only a paththrough for an output Unit)
                        answers.Add(new Answer(flow.GetValue<string>(CorrectAnswers[i]), flow.GetValue<string>(CorrectAnswersFeedback[i]), true));
                        correctAnswersToCheck.Add(flow.GetValue<string>(CorrectAnswers[i]));
                    }
                    for (int i = 0; i < wrongAnswers; i++)
                    {
                        //Output flow (this is only a paththrough for an output Unit)
                        answers.Add(new Answer(flow.GetValue<string>(WrongAnswers[i]), flow.GetValue<string>(WrongAnswersFeedback[i]), false));
                        incorrectAnswersToCheck.Add(flow.GetValue<string>(WrongAnswers[i]));
                    }
                    StatemachineConnector.Instance.StartQuestionUI(QuestionUITypes.Question, flow.GetValue<string>(Question), answers);
                    //GameObject.FindObjectOfType<QuestionnaireController>().InitQuestion(flow.GetValue<string>(Question), answers);
                    break;
                case TrainARUIActionChoices.ListSelection:
                    for (int i = 0; i < correctAnswers; i++)
                    {
                        //Output flow (this is only a paththrough for an output Unit)
                        answers.Add(new Answer(flow.GetValue<string>(CorrectAnswers[i]), " ", true));
                        correctAnswersToCheck.Add(flow.GetValue<string>(CorrectAnswers[i]));
                    }
                    for (int i = 0; i < wrongAnswers; i++)
                    {
                        //Output flow (this is only a paththrough for an output Unit)
                        answers.Add(new Answer(flow.GetValue<string>(WrongAnswers[i]), flow.GetValue<string>(WrongAnswersFeedback[i]), false));
                    }
                    StatemachineConnector.Instance.StartQuestionUI(QuestionUITypes.QuestionList, flow.GetValue<string>(Question), answers);
                    //GameObject.FindObjectOfType<QuestionnaireController>().InitQuestionList(flow.GetValue<string>(Question), answers);
                    break;
                default:
                    break;
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
            if (stateInformation.interactionType != InteractionType.Custom)
            {
                Flow.New(graphReference).Invoke(IncorrectAction[0]);
                return false; 
            }
            switch (actionChoice)
            {
                case TrainARUIActionChoices.InputField:
                    if (correctAnswersToCheck.Contains(stateInformation.parameter))
                        Flow.New(graphReference).Invoke(CorrectAction[0]);
                    else
                    {
                        Flow.New(graphReference).Invoke(IncorrectAction[0]);
                        return false;
                    }
                    break;
                case TrainARUIActionChoices.Questionnaire:
                    if (correctAnswersToCheck.Contains(stateInformation.parameter))
                    {
                        int correctAnswerIndex = correctAnswersToCheck.IndexOf(stateInformation.parameter);
                        Flow.New(graphReference).Invoke(CorrectAction[correctAnswerIndex]);
                    }
                    else if(incorrectAnswersToCheck.Contains(stateInformation.parameter))
                    {
                        int incorrectAnswerIndex = incorrectAnswersToCheck.IndexOf(stateInformation.parameter);
                        Flow.New(graphReference).Invoke(IncorrectAction[incorrectAnswerIndex]);
                        return false;
                    }
                    break;
                case TrainARUIActionChoices.ListSelection:
                    Flow.New(graphReference).Invoke(CorrectAction[0]);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return true;
        }
        /// <summary>
        /// Transform a given number into a string.
        /// Example "1" becomes "1st".
        /// </summary>
        /// <param name="value">Number to change</param>
        /// <returns></returns>
        private string NumberToString(int value)
        {
            string returnString = value + "th";
            switch (value)
            {
                case 1:
                    returnString = "1st";
                    break;
                case 2:
                    returnString = "2nd";
                    break;
                case 3:
                    returnString = "3rd";
                    break;
                default:
                    break;
            }
            return returnString;
        }
    }
}
