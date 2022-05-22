using Static;
using UnityEngine;

namespace Others
{
    /// <summary>
    /// Developer Utility. The StateRequestSimulator allows to test the visual scripting stateflows in the editor by simulating
    /// requests to the statemachine manually. This is attached to the framework but not active by default.
    /// To use it, the PlayModeButtonOverride has to be deactivated/deleted so the playmode can be entered. The StateRequestSimulator
    /// is attached to the root object of the TrainAR framework and can then be used in the Hierarchy.
    /// </summary>
    public class StateRequestSimulator : MonoBehaviour
    {
        /// <summary>
        /// What kind of interaction should be requested.
        /// </summary>
        /// <value>Default is Interact.</value>
        [Tooltip("What kind of interaction should be requested.")]
        public InteractionType interactionType = InteractionType.Interact;
        /// <summary>
        /// What is the TrainAR object name of the first object.
        /// </summary>
        /// <value>Default is "".</value>
        [Tooltip("What is the gameObject name of the first object.")]
        public string primaryObjectName = "";
        /// <summary>
        /// What is the TrainAR object name of the second object.
        /// </summary>
        /// <value>Default is "".</value>
        [Tooltip("What is the gameObject name of the second object.")]
        public string secondaryObjectName = ""; //only used for Combining objects
        /// <summary>
        /// What parameter should be requested.
        /// </summary>
        /// <value>Default is "".</value>
        [Tooltip("What parameter should be requested.")]
        public string parameter = "";
        /// <summary>
        /// GameObject references for the requestStateChange.
        /// </summary>
        /// <value>Default is null.</value>
        private GameObject firstGameObject = null;
        /// <summary>
        /// GameObject references for the requestStateChange.
        /// </summary>
        /// <value>Default is null.</value>
        private GameObject secondGameObject = null;
#if UNITY_EDITOR
    // Update is called once per frame
    public void SimulateStateChangeRequest()
    {
        StatemachineConnector.Instance.RequestStateChange(new StateInformation(primaryObjectName, secondaryObjectName, interactionType, parameter, firstGameObject, secondGameObject));
    }
#endif
    }
}
