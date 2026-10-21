using Others;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// The home screen: lists every training of the build (preview, name, description, round expert) and starts the
    /// chosen one's onboarding. Below the trainings follows the optional TrainAR footer (links, creator, funding, contact).
    /// </summary>
    public class HomeScreen : MonoBehaviour
    {
        /// <summary>
        /// The list the entries are added to (inside a scroll view).
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("The list the entries are added to (inside a scroll view).")]
        private RectTransform list;
        /// <summary>
        /// The inactive entry that is copied per training (Button with "Preview", "Name", "Description", "Expert/Picture").
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("The inactive entry that is copied per training (Button with Preview, Name, Description, Expert/Picture).")]
        private GameObject entryTemplate;
        /// <summary>
        /// Shown while the onboarding loads.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("Shown while the onboarding loads.")]
        private GameObject loadingScreen;
        /// <summary>
        /// The TrainAR footer below the trainings, shown if the catalog says so.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("The TrainAR footer below the trainings, shown if the catalog says so.")]
        private GameObject footer;

        /// <summary>
        /// Phones run Unity apps at 30 frames per second unless told otherwise, which makes scrolling feel sluggish. The
        /// AR training sets its own rate (the camera's); the menus run at the screen's highest refresh rate, at least 60
        /// (e.g. 120 on ProMotion iPhones), from the start and after every training.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void SmoothMenus() =>
            Application.targetFrameRate = Mathf.Max(60, Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value));

        private void Start()
        {
            SmoothMenus(); //Ensure high framerate nased on refrech rate
            var catalog = TrainingCatalog.Instance;
            if (catalog == null) return;
            if (footer != null) footer.SetActive(catalog.showFooter);
            MoveBelowNotch();
            foreach (var training in catalog.trainings)
            {
                if (training == null) continue;
                var entry = Instantiate(entryTemplate, list);
                // directly above the footer, in the catalog's order (the cards' shadows insert themselves before them)
                if (footer != null) entry.transform.SetSiblingIndex(footer.transform.GetSiblingIndex());
                else entry.transform.SetAsLastSibling();
                entry.SetActive(true);
                entry.transform.Find("Name").GetComponent<TMP_Text>().text = training.displayName;
                entry.transform.Find("Description").GetComponent<TMP_Text>().text = training.description;
                var preview = entry.transform.Find("Preview").GetComponent<Image>();
                preview.sprite = training.preview;
                preview.enabled = training.preview != null;
                var expert = entry.transform.Find("Expert/Picture").GetComponent<Image>();
                if (training.expert != null) expert.sprite = training.expert;
                var chosen = training;
                entry.GetComponent<Button>().onClick.AddListener(() => Choose(chosen));
            }
        }

        /// <summary>
        /// Moves the list down by the notch (the part of the screen above the safe area), so the space between the notch
        /// and the logo stays the same as on phones without one.
        /// </summary>
        private void MoveBelowNotch()
        {
            var layout = list.GetComponent<VerticalLayoutGroup>();
            var canvasRect = (RectTransform)GetComponentInParent<Canvas>().rootCanvas.transform;
            var notch = (Screen.height - Screen.safeArea.yMax) / Screen.height * canvasRect.rect.height;
            layout.padding.top += Mathf.RoundToInt(notch);
            LayoutRebuilder.MarkLayoutForRebuild(list);
        }

        /// <summary>
        /// Opens a link of the footer (web page or mail).
        /// </summary>
        public void OpenUrl(string url) => Application.OpenURL(url);

        private void Choose(TrainingInfo training)
        {
            TrainingSelection.Current = training;
            if (loadingScreen != null) loadingScreen.SetActive(true);
            SceneManager.LoadScene("Scenes/Tutorial", LoadSceneMode.Single);
        }
    }
}
