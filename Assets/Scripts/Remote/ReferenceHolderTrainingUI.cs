using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Remote
{
    /// <summary>
    /// Holds the references for the blueprint scenario UI.
    /// </summary>
    public class ReferenceHolderTrainingUI : MonoBehaviour
    {
        /// <summary>
        /// Reference to the TextMeshPro text of the scenario name.
        /// </summary>
        [SerializeField]
        public TextMeshProUGUI scenarioName;
        /// <summary>
        /// Reference to the TextMeshPro text of the scenario description.
        /// </summary>
        [SerializeField]
        public TextMeshProUGUI description;
        /// <summary>
        /// Reference to the background image.
        /// </summary>
        [SerializeField]
        public RawImage background;
        /// <summary>
        /// Reference to the start scenario Button.
        /// </summary>
        [SerializeField]
        public Button startScenario;
        /// <summary>
        /// Reference to the delete scenario Button.
        /// </summary>
        [SerializeField]
        public Button deleteScenario;
    }
}