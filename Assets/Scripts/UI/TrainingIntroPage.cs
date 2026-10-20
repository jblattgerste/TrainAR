using Others;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// The first onboarding page: shows the chosen training's name, its expert presenting the description (as on the end
    /// screen) and the preview.
    /// </summary>
    public class TrainingIntroPage : MonoBehaviour
    {
        /// <summary>
        /// The page's title.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("The page's title.")]
        private TMP_Text title;
        /// <summary>
        /// The page's text.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("The page's text.")]
        private TMP_Text description;
        /// <summary>
        /// The picture of the training's start setup.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("The picture of the training's start setup.")]
        private Image preview;
        /// <summary>
        /// The expert's round picture next to the description (like on the end screen).
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("The expert's round picture next to the description (like on the end screen).")]
        private Image expert;

        private void Awake()
        {
            var training = TrainingSelection.Current;
            if (training == null) return;
            title.text = training.displayName;
            description.text = training.description;
            preview.sprite = training.preview;
            preview.enabled = training.preview != null;
            if (expert != null && training.expert != null) expert.sprite = training.expert;
        }
    }
}
