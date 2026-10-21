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
        [SerializeField] private TMP_InputField inputField;
        private bool buttonsFaded = false;
        private StatemachineConnector subscribedConnector;
        private Coroutine pendingInputSubmission;
        private bool inputSubmissionQueued;
        private bool inputQuestionActive;
        private int questionVersion;

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
        [SerializeField] private AudioSource audioSource;

        /// <summary>
        /// How long the feedback of a chosen answer stays open before the questionnaire continues on its own.
        /// Tapping the feedback or the question closes it earlier.
        /// </summary>
        /// <value>Default is 5 seconds.</value>
        [Header("Feedback")]
        public float feedbackDuration = 5f;
        /// <summary>
        /// Color of the bar on top of the question that fills up while the feedback is open.
        /// </summary>
        /// <value>Default is the light teal of the TrainAR logo.</value>
        public Color feedbackTimerColor = new Color(0.5490196f, 0.7686275f, 0.8f, 1f);
        /// <summary>
        /// Color of the not yet filled part of the feedback timer bar.
        /// </summary>
        /// <value>Default is a dark shade of the UI blue.</value>
        public Color feedbackTimerBackgroundColor = new Color(0f, 0.2f, 0.33f, 1f);
        /// <summary>
        /// Height of the feedback timer bar in canvas units.
        /// </summary>
        /// <value>Default is 24.</value>
        public float feedbackTimerHeight = 24f;
        private GameObject feedbackTimerBar;
        private RectTransform feedbackTimerFill;
        /// <summary>
        /// True once an answer of the current regular question was chosen.
        /// </summary>
        private bool answerChosen;
        /// <summary>
        /// True while the feedback of the chosen answer is fully shown and can be closed.
        /// </summary>
        private bool feedbackOpen;
        private Coroutine feedbackCountdown;
        /// <summary>
        /// Invisible full-screen button that closes the feedback of a wrong list answer on any tap.
        /// </summary>
        private GameObject listFeedbackCloser;

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
        /// Sets the input model reference, including for scenes created before it was serialized.
        /// </summary>
        private void Awake()
        {
            if (inputField == null && inputFieldQuestionUI != null)
                inputField = inputFieldQuestionUI.GetComponentInChildren<TMP_InputField>(true);
        }

        private void OnEnable()
        {
            subscribedConnector = StatemachineConnector.Instance;
            subscribedConnector.TriggerUIQuestion += InitTypeOfQuestion;
        }
        /// <summary>
        /// Removes listener for UIQuestion event.
        /// </summary>
        private void OnDisable()
        {
            if (subscribedConnector != null)
                subscribedConnector.TriggerUIQuestion -= InitTypeOfQuestion;
            subscribedConnector = null;
            questionVersion++;
            CancelPendingInputSubmission();
            CancelFeedbackCountdown();
            feedbackOpen = false;
        }

        private void CancelPendingInputSubmission()
        {
            if (pendingInputSubmission != null)
                StopCoroutine(pendingInputSubmission);
            pendingInputSubmission = null;
            inputSubmissionQueued = false;
        }

        private void BeginQuestion(bool isInputQuestion)
        {
            questionVersion++;
            CancelPendingInputSubmission();
            inputQuestionActive = isInputQuestion;
            if (!isInputQuestion)
                inputFieldQuestionUI.SetActive(false);
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
            BeginQuestion(false);
            answerChosen = false;
            feedbackOpen = false;
            regularQuestionUI.SetActive(true);
            ResetAnswerButtons();
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
            BeginQuestion(true);
            inputField.SetTextWithoutNotify(string.Empty);
            inputFieldQuestionText.text = question;
            inputFieldQuestionUI.SetActive(true);
        }
        /// <summary>
        /// Open the questionList UI.
        /// </summary>
        /// <param name="question">Text of the question.</param>
        /// <param name="answers">List of all possbile answers.</param>
        public void InitQuestionList(string question, List<Answer> answers)
        {
            BeginQuestion(false);
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
            if (audioSource == null)
                audioSource = FindAnyObjectByType<AudioSource>();
            InitRegularQuestionButtons();
            InitQuestionTapToContinue();
            CreateFeedbackTimerBar();
            questionnaireUIDefaultPosition = questionnaireUIRectTransform.anchoredPosition;
        }

        /// <summary>
        /// Lets a tap on the question panel close the open feedback as well, so the whole questionnaire UI
        /// (question and feedback) can be tapped, but not the AR scene behind it.
        /// </summary>
        private void InitQuestionTapToContinue()
        {
            EventTrigger trigger = questionTextUI.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            entry.callback.AddListener(_ => ContinueButtonOnClick());
            trigger.triggers.Add(entry);
        }

        /// <summary>
        /// Creates the bar on top of the question panel that fills up from left to right while the feedback is open.
        /// </summary>
        private void CreateFeedbackTimerBar()
        {
            feedbackTimerBar = new GameObject("Feedback Timer Bar", typeof(RectTransform), typeof(Image));
            feedbackTimerBar.layer = questionTextUI.layer;
            RectTransform barRect = feedbackTimerBar.GetComponent<RectTransform>();
            barRect.SetParent(questionTextUI.transform, false);
            barRect.anchorMin = new Vector2(0f, 1f);
            barRect.anchorMax = new Vector2(1f, 1f);
            barRect.pivot = new Vector2(0.5f, 0f);
            barRect.anchoredPosition = Vector2.zero;
            barRect.sizeDelta = new Vector2(0f, feedbackTimerHeight);
            Image background = feedbackTimerBar.GetComponent<Image>();
            background.color = feedbackTimerBackgroundColor;
            background.raycastTarget = false;

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.layer = questionTextUI.layer;
            feedbackTimerFill = fill.GetComponent<RectTransform>();
            feedbackTimerFill.SetParent(barRect, false);
            feedbackTimerFill.anchorMin = Vector2.zero;
            feedbackTimerFill.anchorMax = new Vector2(0f, 1f);
            feedbackTimerFill.offsetMin = Vector2.zero;
            feedbackTimerFill.offsetMax = Vector2.zero;
            Image fillImage = fill.GetComponent<Image>();
            fillImage.color = feedbackTimerColor;
            fillImage.raycastTarget = false;

            feedbackTimerBar.SetActive(false);
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
            // Once an answer was chosen, only its feedback card stays interactable and tapping it closes the feedback.
            if (answerChosen)
            {
                ContinueButtonOnClick();
                return;
            }
            answerChosen = true;
            DisableAllButtons(buttons);
            // Keep the chosen button interactable, so it can be tapped and is not dimmed by the disabled tint.
            buttons[i].interactable = true;
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
                ShowListFeedbackCloser(button);
            }
        }

        /// <summary>
        /// While a wrong answer of a list question shows its feedback, a tap on the question or the list closes it (like
        /// tapping the answer itself), as for the feedback of regular questions; the rest of the screen is not affected.
        /// </summary>
        /// <param name="openAnswer">The answer that shows its feedback.</param>
        private void ShowListFeedbackCloser(Button openAnswer)
        {
            // the list question's UI: the parent of the answers' scroll view (question and list)
            var scroll = openAnswer.GetComponentInParent<ScrollRect>();
            var area = scroll != null && scroll.transform.parent != null ? scroll.transform.parent : scroll.transform;
            if (listFeedbackCloser == null)
            {
                listFeedbackCloser = new GameObject("List Feedback Closer", typeof(RectTransform), typeof(Image), typeof(Button),
                    typeof(LayoutElement));
                listFeedbackCloser.layer = area.gameObject.layer;
                listFeedbackCloser.GetComponent<LayoutElement>().ignoreLayout = true;
                var rect = (RectTransform)listFeedbackCloser.transform;
                rect.SetParent(area, false);
                listFeedbackCloser.GetComponent<Image>().color = Color.clear;
                listFeedbackCloser.GetComponent<Button>().transition = Selectable.Transition.None;
            }
            // cover exactly the question text and the list (the parent itself is only a small anchor)
            var closerRect = (RectTransform)listFeedbackCloser.transform;
            var min = new Vector2(float.MaxValue, float.MaxValue);
            var max = new Vector2(float.MinValue, float.MinValue);
            var corners = new Vector3[4];
            foreach (RectTransform child in area)
            {
                if (child == closerRect || !child.gameObject.activeSelf) continue;
                child.GetWorldCorners(corners);
                foreach (var corner in corners)
                {
                    Vector2 local = area.InverseTransformPoint(corner);
                    min = Vector2.Min(min, local);
                    max = Vector2.Max(max, local);
                }
            }
            closerRect.anchorMin = closerRect.anchorMax = closerRect.pivot = new Vector2(0.5f, 0.5f);
            closerRect.anchoredPosition = (min + max) / 2 - ((RectTransform)area).rect.center;
            closerRect.sizeDelta = max - min;
            listFeedbackCloser.transform.SetAsLastSibling();
            var closer = listFeedbackCloser.GetComponent<Button>();
            closer.onClick.RemoveAllListeners();
            closer.onClick.AddListener(() =>
            {
                listFeedbackCloser.SetActive(false);
                openAnswer.onClick.Invoke();
            });
            listFeedbackCloser.SetActive(true);
        }
        /// <summary>
        /// Submit the current input model from an explicit submit button or other caller.
        /// Do not connect this to End Edit, which also fires for cancellation and focus loss.
        /// </summary>
        public void QuestionInputValidate()
        {
            if (inputField != null)
                OnInputFieldSubmitted(inputField.text);
        }

        /// <summary>
        /// Receives TMP's Submit event (Return or the mobile keyboard's Done action).
        /// The raw model preserves exact answers, including an explicitly configured empty answer.
        /// </summary>
        public void OnInputFieldSubmitted(string answer)
        {
            if (!isActiveAndEnabled || !inputQuestionActive || inputSubmissionQueued ||
                inputField == null || !inputFieldQuestionUI.activeInHierarchy)
                return;

            inputSubmissionQueued = true;
            pendingInputSubmission = StartCoroutine(SubmitInputAfterEditing(answer ?? string.Empty, questionVersion));
        }

        private IEnumerator SubmitInputAfterEditing(string answer, int submittedQuestionVersion)
        {
            // TMP sends Submit before closing its keyboard. Let that teardown finish before the graph
            // can open another question using the same field.
            yield return null;
            pendingInputSubmission = null;

            try
            {
                if (!isActiveAndEnabled || submittedQuestionVersion != questionVersion ||
                    !inputQuestionActive || !inputFieldQuestionUI.activeInHierarchy)
                    yield break;

                inputField.DeactivateInputField();
                if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject == inputField.gameObject)
                    EventSystem.current.SetSelectedGameObject(null);

                // Other focus callbacks may also replace the question.
                if (submittedQuestionVersion != questionVersion || !isActiveAndEnabled)
                    yield break;

                inputQuestionActive = false;
                inputFieldQuestionUI.SetActive(false);
                NotifyStatemachine(answer, QuestionUITypes.InputQuestion);
            }
            finally
            {
                // A synchronous graph transition can already have queued the next question's answer.
                if (submittedQuestionVersion == questionVersion)
                    inputSubmissionQueued = false;
            }
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
            int submittedQuestionVersion = questionVersion;
            ARInteractButtons.ActivateInteractButtons();
            bool temp = StatemachineConnector.Instance.RequestStateChange(new StateInformation("", "", InteractionType.Custom, value));
            if (QuestionUITypes.InputQuestion == type)
            {
                if (temp) PlayCorrectAnswerSound();
                else PlayWrongAnswerSound();

                // The graph runs synchronously and may have initialized a different question.
                // Never clear its answer or reopen the previous question after that transition.
                if (!isActiveAndEnabled || submittedQuestionVersion != questionVersion)
                    return;

                if (temp)
                {
                    inputQuestionActive = false;
                    inputField.SetTextWithoutNotify(string.Empty);
                }
                else
                {
                    inputQuestionActive = true;
                    inputField.SetTextWithoutNotify(value);
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
        /// Play the audioclip for correct answers as a one-shot, so it does not cut off other sounds.
        /// </summary>
        private void PlayCorrectAnswerSound()
        {
            audioSource.PlayOneShot(correctAnswerSound);
        }
        /// <summary>
        /// Play the audioclip for wrong answers as a one-shot, so it does not cut off other sounds.
        /// </summary>
        private void PlayWrongAnswerSound()
        {
            audioSource.PlayOneShot(wrongAnswerSound);
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

                // Set the pressed button global, the continue button needs this.
                chosenButtonIndex = buttonIndex;
            }
            else
            {
                // If the pressed button was wrong, change the buttons color to red.
                buttons[buttonIndex].gameObject.GetComponent<Image>().color = wrongAnswerColor;
                // Set the pressed button global, the continue button needs this.
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
            // Ignore taps before the feedback is fully shown and after it was closed (countdown and tap can coincide).
            if (!feedbackOpen) return;
            feedbackOpen = false;
            CancelFeedbackCountdown();
            StartCoroutine(ReverseAnswerButtonAndInitiateNextState());
            Debug.Log("Feedback: Weiter nach Feedback");
        }

        /// <summary>
        /// Fills the timer bar on top of the question while the feedback is open and closes the feedback when it is full.
        /// </summary>
        /// <returns></returns>
        private IEnumerator CloseFeedbackAfterCountdown()
        {
            feedbackTimerBar.SetActive(true);
            for (float elapsed = 0f; elapsed < feedbackDuration; elapsed += Time.deltaTime)
            {
                feedbackTimerFill.anchorMax = new Vector2(elapsed / feedbackDuration, 1f);
                yield return null;
            }
            feedbackTimerFill.anchorMax = Vector2.one;
            feedbackCountdown = null;
            ContinueButtonOnClick();
        }

        /// <summary>
        /// Stops a running feedback countdown and hides the timer bar.
        /// </summary>
        private void CancelFeedbackCountdown()
        {
            if (feedbackCountdown != null)
                StopCoroutine(feedbackCountdown);
            feedbackCountdown = null;
            if (feedbackTimerBar != null) feedbackTimerBar.SetActive(false);
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

            // Make the other buttons fully visible again, otherwise they keep a faded alpha when the UI is reactivated
            foreach (Button b in buttons)
            {
                CanvasGroup group = b.GetComponent<CanvasGroup>();
                if (group != null) group.alpha = 1f;
            }

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
            feedbackOpen = true;
            feedbackCountdown = StartCoroutine(CloseFeedbackAfterCountdown());
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
        /// Resets the answer buttons to be fully visible with the default color and rebinds their animators,
        /// so no faded alpha or feedback color from the previous question is kept.
        /// </summary>
        private void ResetAnswerButtons()
        {
            foreach (Button b in buttons)
            {
                b.GetComponent<Image>().color = defaultUIColor;
                CanvasGroup group = b.GetComponent<CanvasGroup>();
                if (group != null) group.alpha = 1f;
                Animator buttonAnimator = b.animator;
                if (buttonAnimator == null || !buttonAnimator.isActiveAndEnabled) continue;
                buttonAnimator.SetBool("fade", false);
                buttonAnimator.SetBool("open", false);
                buttonAnimator.Rebind();
                buttonAnimator.Update(0f);
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
