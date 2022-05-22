using System;
using Interaction;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Handles interactions with the interact button.
    /// </summary>
    public class InteractButton : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Reference to the Interaction Controller.")]
        private InteractionController interactionController;
        [SerializeField]
        [Tooltip("The text displayed on the interact button.")]
        private TextMeshProUGUI buttonText;

        private void Start()
        {
            buttonText.text = "Interact";
        }
        private void Update()
        {
            if (interactionController.isIntersecting)
            {
                buttonText.text = "Combine";
                buttonText.text = setCombineString();
            }
            else
            {
                buttonText.text = "Interact";
            }
        }

        /// <summary>
        /// Calls either an interact or an combine, depending on the interactioncontroller context.
        /// </summary>
        public void ReleaseInteract()
        {
            //When grabbing Object and intersecting, Combine.
            if(interactionController.isGrabbingObject && interactionController.isIntersecting)
            {
                interactionController.Combine();
            }
            else
                //Otherwise, it's always an interact.
            {
                interactionController.selectedObject.GetComponent<TrainARObject>().Interact();
            }
        }

        private string setCombineString()
        {
            string result = "Combine";
            String nameGrabbedObject = interactionController.grabbedObject.GetComponent<TrainARObject>().interactableName;
            String nameIntersectedObject = interactionController.intersectedObject.GetComponent<TrainARObject>().interactableName;
            return result;
        }
    }
}
