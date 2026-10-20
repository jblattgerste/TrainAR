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

        private void Start()
        {
            var catalog = TrainingCatalog.Instance;
            if (catalog == null) return;
            if (footer != null) footer.SetActive(catalog.showFooter);
            var index = entryTemplate.transform.GetSiblingIndex();
            foreach (var training in catalog.trainings)
            {
                if (training == null) continue;
                var entry = Instantiate(entryTemplate, list);
                entry.transform.SetSiblingIndex(++index);           // above the footer
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
