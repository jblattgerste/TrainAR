using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine.EventSystems;
using Static;
using UI;
using UnityEditor;

namespace UI
{
    /// <summary>
    /// The QuestionaireController provides the functionality to control the input field questions, reqular question and question lists.
    /// The input field question opens the keyboard and recives any given string input from the user.
    /// The regular question opens a question with up to four possible pre-defined awnsers.
    /// The question list opens a list with n possible items and continues when all right items were selected.
    /// </summary>
    public class QuestionnaireController : MonoBehaviour
    {
        /// <summary>
        /// Index of pressed button.
        /// </summary>
        /// <value>Set on runtime.</value>
        private int chosenButtonIndex;
        /// <summary>
        /// How long it takes to fade in the UI.
        /// </summary>
        /// <value>Default is 1.0f seconds.</value>
        public float duration = 1.0f;
        /// <summary>
        /// List of answers for opened question.
        /// </summary>
        /// <value>Set on runtime.</value>
        private List<Answer> answers;
        /// <summary>
        /// Reference to the InteractionButtonContoller.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        private InteractionButtonController ARInteractButtons;
        /// <summary>
        /// Reference to the root gameObject of the inputFieldUI.
        /// </summary>
        /// <value>Set in inspector.</value>
        [Header("Input Field UI")]
        public GameObject inputFieldQuestionUI;
        /// <summary>
        /// Reference to the inputFieldQuestionText.
        /// </summary>
        /// <value>Set in inspector.</value>
        public TextMeshProUGUI inputFieldQuestionText;
        /// <summary>
        /// Reference to the inputFieldAnswerText.
        /// </summary>
        /// <value>Set in inspector.</value>
        public TextMeshProUGUI inputFieldAnswerText;
        private bool buttonsFaded = false;
        /// <summary>
        /// Reference to the keyboard.
        /// </summary>
        /// <value>Set on runtime.</value>
        private TouchScreenKeyboard keyboard;
        /// <summary>
        /// Reference to the inputFieldRect.
        /// </summary>
        /// <value>Set on runtime.</value>
        private RectTransform inputFieldRect;

        /// <summary>
        /// Reference to the root gameObject of the regularQuestionUI.
        /// </summary>
        /// <value>Set in inspector.</value>
        [Header("Regular Question UI")]
        public GameObject regularQuestionUI;
        /// <summary>
        /// Reference to the questionText.
        /// </summary>
        /// <value>Set in inspector.</value>
        public GameObject questionTextUI;
        /// <summary>
        /// Reference to the UIRectTransform.
        /// </summary>
        /// <value>Set in inspector.</value>
        public RectTransform questionnaireUIRectTransform;
        /// <summary>
        /// Stores the questionaireUIDefault position.
        /// </summary>
        /// <value>Set on runtime.</value>
        private Vector2 questionnaireUIDefaultPosition;
        /// <summary>
        /// References to the Buttons
        /// </summary>
        /// <value>Set in inspector.</value>
        public List<Button> buttons;
        /// <summary>
        /// Reference to the questionText.
        /// </summary>
        /// <value>Set in inspector.</value>
        public TextMeshProUGUI questionText;
        /// <summary>
        /// Reference to the ResponseText.
        /// </summary>
        /// <value>Set in inspector.</value>
        public GameObject responseText;

        /// <summary>
        /// Reference to the root of the question list UI.
        /// </summary>
        /// <value>Set in inspector.</value>
        [Header("QuestionList UI")]
        public GameObject questionList;
        /// <summary>
        /// Reference to the QuestionListText.
        /// </summary>
        /// <value>Set in inspector.</value>
        public TextMeshProUGUI questionListQuestionText;
        /// <summary>
        /// Reference to the AnswerButtonPrefab.
        /// </summary>
        /// <value>Set in inspector.</value>
        public Button ButtonBlueprint;
        /// <summary>
        /// List of answerButtons.
        /// </summary>
        /// <value>Set on runtime.</value>
        private List<Button> listOfQuestionListButtons;
        /// <summary>
        /// Stores which correct answers are pressed.
        /// </summary>
        /// <value>Set on runtime.</value>
        private List<bool> listProgressCheck;

        /// <summary>
        /// Color for correct answers.
        /// </summary>
        /// <value>Default set in inspector.</value>
        [Header("Colors")]
        public Color correctAnswerColor;
        /// <summary>
        /// Color for wrong answers.
        /// </summary>
        /// <value>Default set in inspector.</value>
        public Color wrongAnswerColor;
        /// <summary>
        /// Color for unpressed buttons.
        /// </summary>
        /// <value>Default set in inspector.</value>
        public Color defaultUIColor;
        /// <summary>
        /// Holder if a correct answer is pressed.
        /// </summary>
        /// <value>Set on runtime.</value>
        private bool correctAnswerChosen;

        /// <summary>
        /// Audioclip that is played when correct answer is pressed.
        /// </summary>
        /// <value>Default is set in inspector.</value>
        [Header("Audio")]
        public AudioClip correctAnswerSound;
        /// <summary>
        /// Audioclip that is played when wrong answer is pressed.
        /// </summary>
        /// <value>Default is set in inspector.</value>
        public AudioClip wrongAnswerSound;
        /// <summary>
        /// Reference to audioSource where audioClips are played.
        /// </summary>
        /// <value>Set in inspector.</value>
        private AudioSource audioSource;

        /// <summary>
        /// Stores wrong answered questions for endscreen UI.
        /// </summary>
        /// <value>Set on runtime.</value>
        public List<string> wrongAnsweredQuestions;
        /// <summary>
        /// Stores wrong answers for endscreen UI.
        /// </summary>
        /// <value>Set on runtime.</value>
        public List<string> wrongAnswers;
        /// <summary>
        /// Stores right answers for endscreen UI.
        /// </summary>
        /// <value>Set on runtime.</value>
        public List<string> rightAnswer;

        /// <summary>
        /// Holder for information for one answer.
        /// </summary>
        public struct Answer
        {
            /// <summary>
            /// Text of the answer.
            /// </summary>
            /// <value>Set by constructor.</value>
            public string answer;
            /// <summary>
            /// Text of the feedback.
            /// </summary>
            /// <value>Set by constructor.</value>
            public string answerFeedback;
            /// <summary>
            /// Is this answer right or wrong.
            /// </summary>
            /// <value>Set by constructor.</value>
            public bool answerCorrect;
            /// <summary>
            /// Constructor for Answers.
            /// </summary>
            /// <param name="answer">Text of the answer.</param>
            /// <param name="answerFeedback">Text for feedback when the answer was taken.</param>
            /// <param name="answerCorrect">True if the answer is right.</param>
            public Answer(string answer, string answerFeedback, bool answerCorrect)
            {
                this.answer = answer;
                this.answerFeedback = answerFeedback;
                this.answerCorrect = answerCorrect;
            }
        }

        /// <summary>
        /// The type of the UI custom action/question.
        /// </summary>
        public enum QuestionUITypes
        {
            Question,
            QuestionList,
            InputQuestion
        }
        /// <summary>
        /// Sets missing references and adds listener for UIQuestion event.
        /// </summary>
        private void Awake()
        {
            inputFieldRect = inputFieldQuestionUI.GetComponent<RectTransform>();
            StatemachineConnector.Instance.TriggerUIQuestion += InitTypeOfQuestion;
        }
        /// <summary>
        /// Removes listener for UIQuestion event.
        /// </summary>
        private void OnDisable()
        {
            StatemachineConnector.Instance.TriggerUIQuestion -= InitTypeOfQuestion;
        }

        /// <summary>
        /// Starts the question UI depending of the given type input.
        /// </summary>
        /// <param name="type">Defines the type of question that is opened.</param>
        /// <param name="question">Text of the question.</param>
        /// <param name="answers">List all possible answers.</param>
        private void InitTypeOfQuestion(QuestionUITypes type, string question, List<Answer> answers)
        {
            ARInteractButtons.DeactivateInteractButtons();
            this.answers = answers;
            switch (type)
            {
                case QuestionUITypes.Question:
                    InitQuestion(question, answers);
                    break;
                case QuestionUITypes.QuestionList:
                    InitQuestionList(question, answers);
                    break;
                case QuestionUITypes.InputQuestion:
                    InitInputfield(question);
                    break;
                default:
                    break;
            }
        }
        /// <summary>
        /// Open the regular question UI.
        /// </summary>
        /// <param name="question">Text of the question.</param>
        /// <param name="answers">List of all possible answers.</param>
        public void InitQuestion(string question, List<Answer> answers)
        {
            regularQuestionUI.SetActive(true);
            questionText.text = question;
            questionTextUI.SetActive(true);
            EnableAllButtons(buttons);
            Shuffle(answers);

            if (answers.Count < buttons.Count)
            {
                int diff = buttons.Count - answers.Count;
                for (int i = 1; i <= diff; i++)
                {
                    buttons[buttons.Count - i].GetComponentInChildren<TextMeshProUGUI>().text = "";
                    buttons[buttons.Count - i].interactable = false;
                }
            }

            for (int i = 0; i < answers.Count; i++)
            {
                buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = answers[i].answer;
            }
        }
        /// <summary>
        /// Open the inputField question UI.
        /// </summary>
        /// <param name="question">Text of the question.</param>
        public void InitInputfield(string question)
        {
            inputFieldQuestionUI.SetActive(true);
            inputFieldQuestionText.text = question;
        }
        /// <summary>
        /// Open the questionList UI.
        /// </summary>
        /// <param name="question">Text of the question.</param>
        /// <param name="answers">List of all possbile answers.</param>
        public void InitQuestionList(string question, List<Answer> answers)
        {
            listProgressCheck = new List<bool>();
            listOfQuestionListButtons = new List<Button>();
            questionList.SetActive(true);
            Shuffle(answers);
            questionListQuestionText.text = question;
            for (int i = 0; i < answers.Count; i++)
            {
                GameObject newButton = Instantiate(ButtonBlueprint.gameObject);
                newButton.transform.SetParent(ButtonBlueprint.gameObject.transform.parent, false);
                newButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = answers[i].answer;
                int help = i;
                newButton.GetComponent<Button>().onClick.AddListener(() => ValidateListAnswer(help));
                newButton.SetActive(true);
                listOfQuestionListButtons.Add(newButton.GetComponent<Button>());
                listProgressCheck.Add(false);
            }
        }
        /// <summary>
        /// Sets missing references and sets default button functions.
        /// </summary>
        void Start()
        {
            audioSource = GameObject.FindObjectOfType<AudioSource>();
            InitRegularQuestionButtons();
            questionnaireUIDefaultPosition = questionnaireUIRectTransform.anchoredPosition;
        }

        /// <summary>
        /// Initializes each of the onClick-Listeners of the button the ValidateAnswer()-Function.
        /// </summary>
        private void InitRegularQuestionButtons()
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                Button button = buttons[i];
                int buttonIndex = i;
                button.onClick.AddListener(() => ValidateAnswer(buttonIndex));
            }
        }

        /// <summary>
        /// checks whether the chosen answer for the current regular question was correct.
        /// </summary>
        /// <param name="i">The index of the pressed button.</param>
        private void ValidateAnswer(int i)
        {
            DisableAllButtons(buttons);
            bool currentQ = answers[i].answerCorrect;
            if (currentQ)
            {
                PlayCorrectAnswerSound();
                Debug.Log("Antwort: Richtige Antwort: " + buttons[i].GetComponentInChildren<TextMeshProUGUI>().text);
                ShowResponse(true, i);
            }
            else
            {
                PlayWrongAnswerSound();
                Debug.Log("Antwort: Falsche Antwort: " + buttons[i].GetComponentInChildren<TextMeshProUGUI>().text);
                ShowResponse(false, i);
                SaveWrongAnsweredQuestion(questionText.text);
                SaveWrongAnswer(buttons[i].GetComponentInChildren<TextMeshProUGUI>().text);
            }
        }
        /// <summary>
        /// Checks if the given index of answer is correct or wrong.
        /// </summary>
        /// <param name="value">Index of the button.</param>
        private void ValidateListAnswer(int value)
        {
            if (answers[value].answerCorrect)
            {
                PlayCorrectAnswerSound();
                ListShowResponse(true, value);
                checkIfDone(value);
            }
            else
            {
                PlayWrongAnswerSound();
                ListShowResponse(false, value);
                listOfQuestionListButtons[value].gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = answers[value].answerFeedback;
                listOfQuestionListButtons[value].gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(1440.4f, 400f);
            }
        }
        /// <summary>
        /// Show Response on the questionList UI.
        /// </summary>
        /// <param name="right">True if answer was correct.</param>
        /// <param name="buttonIndex">Index of pressed button.</param>
        private void ListShowResponse(bool right, int buttonIndex)
        {
            DisableAllButtons(listOfQuestionListButtons);
            Button button = listOfQuestionListButtons[buttonIndex];
            if (right)
            {
                button.gameObject.GetComponent<Image>().color = correctAnswerColor;
                StartCoroutine(disableAfterSeconds(button, 0.5f));
            }
            else
            {
                button.gameObject.GetComponent<Image>().color = wrongAnswerColor;
                button.interactable = true;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => button.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = answers[buttonIndex].answer);
                button.onClick.AddListener(() => button.gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(1440.4f, 195f));
                button.onClick.AddListener(() => button.interactable = false);
                button.onClick.AddListener(() => button.enabled = false);
                button.onClick.AddListener(() => EnableAllButtons(listOfQuestionListButtons));
            }
        }
        /// <summary>
        /// Validate if given input of the inputQuestion UI was correct.
        /// </summary>
        public void QuestionInputValidate()
        {
            inputFieldQuestionUI.SetActive(false);
            NotifyStatemachine(inputFieldAnswerText.text.Substring(0, inputFieldAnswerText.text.Length - 1), QuestionUITypes.InputQuestion);
            inputFieldAnswerText.text = "";
        }
        /// <summary>
        /// Coroutine to disable pressed button.
        /// </summary>
        /// <param name="button">Reference to button.</param>
        /// <param name="seconds">Seconds to wait before disableing.</param>
        /// <returns></returns>
        private IEnumerator disableAfterSeconds(Button button, float seconds)
        {
            yield return new WaitForSeconds(seconds);
            if (button != null)
            {
                button.gameObject.SetActive(false);
                EnableAllButtons(listOfQuestionListButtons);
            }
        }
        /// <summary>
        /// Check it all correct questionList answers were pressed.
        /// </summary>
        /// <param name="question">Index of answer.</param>
        private void checkIfDone(int question)
        {
            listProgressCheck[question] = true;
            for (int i = 0; i < listProgressCheck.Count; i++)
            {
                if (answers[i].answerCorrect)
                    if (!listProgressCheck[i]) return;
            }
            ReverseQuestionListToDefault();
        }
        /// <summary>
        /// Remove all created buttons of questionList UI before next question.
        /// </summary>
        private void ReverseQuestionListToDefault()
        {
            // deactivate the questionnaire UI
            questionList.SetActive(false);
            foreach (var item in listOfQuestionListButtons)
            {
                Destroy(item.gameObject);
            }
            // Notify the statemachine.
            NotifyStatemachine("Questionlist all done");
        }

        /// <summary>
        /// Disables all regular question buttons.
        /// </summary>
        private void DisableAllButtons(List<Button> buttonList)
        {
            foreach (Button b in buttonList)
            {
                b.interactable = false;
            }
        }
        /// <summary>
        /// Notify the statemachine with the result of the question UI.
        /// </summary>
        /// <param name="value">Result in text form.</param>
        /// <param name="type">Type of question.</param>
        public void NotifyStatemachine(string value, QuestionUITypes type = QuestionUITypes.Question)
        {
            ARInteractButtons.ActivateInteractButtons();
            bool temp = StatemachineConnector.Instance.RequestStateChange(new StateInformation("", "", InteractionType.Custom, value));
            if (QuestionUITypes.InputQuestion == type)
            {
                if (temp) PlayCorrectAnswerSound();
                else
                {
                    PlayWrongAnswerSound();
                    inputFieldQuestionUI.SetActive(true);
                    ARInteractButtons.DeactivateInteractButtons();
                }
            }
        }

        /// <summary>
        /// Enables all buttons, but disables those, who are marked as such in their Answer object.
        /// </summary>
        private void EnableAllButtons(List<Button> buttonList)
        {
            foreach (Button b in buttonList)
            {
                b.interactable = true;
            }
        }
        /// <summary>
        /// Play the audioclip for correct answers.
        /// </summary>
        private void PlayCorrectAnswerSound()
        {
            audioSource.clip = correctAnswerSound;
            audioSource.Play();
        }
        /// <summary>
        /// Play the audioclip for wrong answers.
        /// </summary>
        private void PlayWrongAnswerSound()
        {
            audioSource.clip = wrongAnswerSound;
            audioSource.Play();
        }

        /// <summary>
        /// Gets called when a answer button of a regular question was pressed
        /// Shows feedback on UI, depending on whether the chosen answer was correct or not.
        /// </summary>
        /// <param name="isCorrect">Was the answer correct?</param>
        /// <param name="buttonIndex">index of the pressed button.</param>
        private void ShowResponse(bool isCorrect, int buttonIndex)
        {

            // Put the buttons Gameobject as last in the hierarchy order, so it is in front of the other buttons.
            buttons[buttonIndex].gameObject.transform.SetAsLastSibling();

            // Start the button extension animation.
            StartCoroutine(ExtendButton(buttonIndex));

            if (isCorrect)
            {
                // If the pressed button was correct, change the buttons color to green.
                buttons[buttonIndex].gameObject.GetComponent<Image>().color = correctAnswerColor;

                // Set this global boolean and the pressed button global, the continue button needs this.
                correctAnswerChosen = true;
                chosenButtonIndex = buttonIndex;
            }
            else
            {
                // If the pressed button was wrong, change the buttons color to red.
                buttons[buttonIndex].gameObject.GetComponent<Image>().color = wrongAnswerColor;
                // Set this global boolean and the pressed button global, the continue button needs this.
                correctAnswerChosen = false;
                chosenButtonIndex = buttonIndex;
            }

        }
        /// <summary>
        /// Save text of wrong answered question to list.
        /// </summary>
        /// <param name="wrongAnsweredQuestion">Text of wrong answered question.</param>
        private void SaveWrongAnsweredQuestion(string wrongAnsweredQuestion)
        {
            wrongAnsweredQuestions.Add(wrongAnsweredQuestion);
        }
        /// <summary>
        /// Save text of wrong answered to list.
        /// </summary>
        /// <param name="givenAnswer">Text of wrong answer.</param>
        private void SaveWrongAnswer(string givenAnswer)
        {
            wrongAnswers.Add(givenAnswer);
        }

        /// <summary>
        /// Shuffles the given list.
        /// </summary>
        /// <param name="list">Lift to be shuffled.</param>
        /// <typeparam name="T">Type of List.</typeparam>
        private void Shuffle<T>(IList<T> list)
        {
            System.Random rng = new System.Random();
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
        /// <summary>
        /// Get's called when the continue button is pressed.
        /// When the chosen answer was correct, the statemachine gets notified.
        /// When the chosen answer was wrong, the question gets displayed again.
        /// </summary>
        public void ContinueButtonOnClick()
        {
            // Was answer correct?
            if (correctAnswerChosen)
            {
                StartCoroutine(ReverseAnswerButtonAndInitiateNextState());
            }
            else
            {
                StartCoroutine(ReverseAnswerButtonAndInitiateNextState());
            }
            Debug.Log("Feedback: Weiter nach Feedback");
        }
        /// <summary>
        /// Coroutine to reverse the pressed button and trigger statechange.
        /// </summary>
        /// <returns></returns>
        private IEnumerator ReverseAnswerButtonAndInitiateNextState()
        {
            // Reverse the button animation
            var anim = buttons[chosenButtonIndex].animator;
            anim.SetBool("open", false);
            UnfadeNotPressedButtons(chosenButtonIndex);
            responseText.SetActive(false);
            yield return WaitForAnimDone(anim, "UnextendButton");

            // deactivate the questionnaire UI
            regularQuestionUI.SetActive(false);
            questionTextUI.SetActive(false);

            // reset the position of the questionnaire UI in case it was moved.
            questionnaireUIRectTransform.anchoredPosition = questionnaireUIDefaultPosition;

            buttons[chosenButtonIndex].gameObject.GetComponent<Image>().color = defaultUIColor;


            // Notify the statemachine.
            NotifyStatemachine(answers[chosenButtonIndex].answer);
        }

        /// <summary>
        /// Extends the button to show the feedback response.
        /// </summary>
        /// <param name="buttonIndex"></param>
        /// <returns>The index of the button that's supposed to be extended.</returns>
        private IEnumerator ExtendButton(int buttonIndex)
        {
            FadeNotPressedButtons(buttonIndex);
            var anim = buttons[buttonIndex].gameObject.GetComponent<Animator>();
            anim.SetBool("open", true);
            yield return WaitForAnimDone(anim, "ExtendButton");
            responseText.SetActive(true);
            responseText.GetComponentInChildren<TextMeshProUGUI>().text = answers[buttonIndex].answerFeedback;
        }
        /// <summary>
        /// Waits and yields a frame, until the given animation is done playing.
        /// </summary>
        /// <param name="animator">The Animator object that plays the animation.</param>
        /// <param name="stateName">The name of the state the animation belongs to.</param>
        /// <returns></returns>
        private IEnumerator WaitForAnimDone(Animator animator, string stateName)
        {
            yield return null;
            while (
                animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 && animator.GetCurrentAnimatorStateInfo(0).IsName(stateName)) //|| animator.GetCurrentAnimatorStateInfo(0).IsName("New State"))
            {
                yield return null;
            }
        }
        /// <summary>
        /// Trigger animation to fadeout of pressed buttons.
        /// </summary>
        /// <param name="buttonIndex">Index of pressed button.</param>
        private void FadeNotPressedButtons(int buttonIndex)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                if (i != buttonIndex)
                {
                    buttons[i].GetComponent<Animator>().SetBool("fade", true);
                }
            }
        }
        /// <summary>
        /// Trigger animation to fadein of pressed button.
        /// </summary>
        /// <param name="buttonIndex">Index of pressed button.</param>
        private void UnfadeNotPressedButtons(int buttonIndex)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                if (i != buttonIndex)
                {
                    buttons[i].GetComponent<Animator>().SetBool("fade", false);
                }
            }
        }
        /// <summary>
        /// Trigger animation to fade all buttons except the pressed.
        /// </summary>
        /// <param name="pressedButtonIndex">Index of pressed button.</param>
        private void FadeAllButtonsExceptPressed(int pressedButtonIndex)
        {
            Debug.Log("FadeAllButtons");
            for (int i = 0; i < buttons.Count; i++)
            {
                if (i != pressedButtonIndex)
                {
                    CanvasGroup thisButtonCanvGroup = buttons[i].gameObject.GetComponent<CanvasGroup>();
                    StartCoroutine(FadeUI(thisButtonCanvGroup, thisButtonCanvGroup.alpha, buttonsFaded ? 1 : 0));
                }
            }
            buttonsFaded = !buttonsFaded;
        }
        /// <summary>
        /// Fade the question UI canvas to make space for the keyboard.
        /// </summary>
        /// <param name="canvasGroup">Reference to the canvas.</param>
        /// <param name="start">Start value.</param>
        /// <param name="end">End value.</param>
        /// <returns></returns>
        private IEnumerator FadeUI(CanvasGroup canvasGroup, float start, float end)
        {
            float counter = 0f;

            while (counter < duration)
            {
                counter += Time.deltaTime;
                //print(counter / duration);
                // canvasGroup.alpha = Mathf.Lerp(start, end, counter / duration);
                canvasGroup.alpha = end * (counter / duration) + start;
            }

            yield return null;
        }



    }
}
