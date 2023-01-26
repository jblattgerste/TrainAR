using Interaction;
using Others;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Handles the behaviour of the interaction buttons, such as enabling and disabling when appropriate
    /// or highlighting the combine button in orange when a combination would be possible.
    /// 
    /// It also allows for grabbed objects to be rotatable.
    /// </summary>
    public class InteractionButtonController : MonoBehaviour
    {
        /// <summary>
        /// Reference to the Interaction Controller.
        /// </summary>
        /// <value>Set in inspector.</value>
        [Header("Script References: ")]
        [SerializeField]
        [Tooltip("Reference to the Interaction Controller.")]
        private InteractionController interactionController;
        /// <summary>
        /// Reference to the Prefab Spawning Controller.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("Reference to the Prefab Spawning Controller.")]
        private PrefabSpawningController spawningController;
        /// <summary>
        /// Reference to the Gameobject holding the Interaction- and Grab-button.
        /// </summary>
        /// <value>Set in inspector.</value>
        [Header("UI Elements: ")]
        [SerializeField]
        [Tooltip("Reference to the Gameobject holding the Interaction- and Grab-button.")]
        private RectTransform ButtonHolder;
        /// <summary>
        /// Reference to the Grab button.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("Reference to the Grab button.")]
        private Button grabButton;
        /// <summary>
        /// Reference to the Interact/Combine button.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("Reference to the Interact/Combine button.")]
        private Button interactButton;
        /// <summary>
        /// The speed that a grabbed object is rotated.
        /// </summary>
        /// <value>Default is 0.2f.</value>
        [Header("Options: ")]
        [Range(0f, 2f)] public float grabbedObjectRotationSpeed = 0.2f;
        /// <summary>
        /// Reference to the text field of the grab button.
        /// </summary>
        /// <value>Set in inspector.</value>
        private TMP_Text grabButtonText;
        /// <summary>
        /// Whether or not the buttonHolder Gameobject was active before prefab respositioning.
        /// </summary>
        private bool buttonHolderActiveFlag = true;
        /// <summary>
        /// Adds listener to prefabSpawned and reposition events.
        /// </summary>
        private void Awake()
        {
            PrefabSpawningController.prefabSpawned += SpawnPrefab;
            PrefabSpawningController.RepositionPrefab += RepositionPrefab;
            grabButtonText = grabButton.GetComponentInChildren<TMP_Text>();
        }
        /// <summary>
        /// Removes listener to prefabSpawned and reposition events.
        /// </summary>
        private void OnDestroy()
        {
            PrefabSpawningController.prefabSpawned -= SpawnPrefab;
            PrefabSpawningController.RepositionPrefab -= RepositionPrefab;
        }
        /// <summary>
        /// Starts updating of the interaction buttons and the rotation.
        /// </summary>
        void Update()
        {
            UpdateButtons();
            TouchRotation();
        }

        /// <summary>
        /// Activates the interaction buttons for the user to see.
        /// </summary>
        public void ActivateInteractButtons()
        {
            ButtonHolder.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Deactivates interact-Button-UI, if it is active while reposition via PrefabSpawningController get's get triggered
        /// Also safes sets a flag, which is used when prefab is later spawned via PrefabSpawningController
        /// </summary>
        private void RepositionPrefab()
        {
            buttonHolderActiveFlag = ButtonHolder.gameObject.activeSelf;
            if (ButtonHolder.gameObject.activeSelf)
                DeactivateInteractButtons();
        }

        private void SpawnPrefab()
        {
            ButtonHolder.gameObject.SetActive(buttonHolderActiveFlag);
        }
        
        /// <summary>
        /// Deactivates the interaction buttons.
        /// </summary>
        public void DeactivateInteractButtons()
        {
            ButtonHolder.gameObject.SetActive(false);
        }
        /// <summary>
        /// Updates the highlighting of the buttons, depending on whether they can be used or not.
        /// </summary>
        private void UpdateButtons()
        {
            //When an object is selected, that object is grabbable, or if a object is currently grabbed, make grab-button interactable. otherwise don't.
            if (interactionController.isSelectingObject &&
                !interactionController.tryedGrabbingObjectUnsuccessfully &&
                interactionController.selectedObject.GetComponent<TrainARObject>().isGrabbable ||
                interactionController.isGrabbingObject)
            {
                grabButton.interactable = true;
            }
            else
            {
                grabButton.interactable = false;
            }
            
            //Check if Object is currently grabbed
            if (interactionController.isGrabbingObject)
            { 
                //check if currently grabbed Object is disabled, if so then release it.
                if (interactionController.grabbedObject.GetComponent<TrainARObject>().TrainARObjectDisabled)
                {
                    GrabRelease();
                }
            }
            
            //When there is a object to select, make interactbutton interactable.
            //interactButton.interactable = interactionController.isSelectingObject;

            //When grabbing object, and intersecting with another object or when selecting object make interactbutton interactable.
            if (interactionController.isIntersecting && interactionController.isGrabbingObject && interactionController.selectedObject.GetComponent<TrainARObject>().isCombineable && interactionController.intersectedObject.GetComponent<TrainARObject>().isCombineable || interactionController.isSelectingObject && interactionController.selectedObject.GetComponent<TrainARObject>().isInteractable && !interactionController.isIntersecting)
            {
                interactButton.interactable = true;
                if (interactionController.isIntersecting)
                {
                    interactButton.gameObject.GetComponent<Image>().color = new Color32(255, 188, 0, 255);
                }
                else
                {
                    interactButton.gameObject.GetComponent<Image>().color = new Color32(0, 106, 173, 255);
                }
            }
            else
            {
                interactButton.interactable = false;
                interactButton.gameObject.GetComponent<Image>().color = new Color32(0, 106, 173, 255);
            }

            //Reset the tryed GrabbingObject
            interactionController.tryedGrabbingObjectUnsuccessfully = false;

            AdjustButtonLabel();
        }
        
        /// <summary>
        /// Handles what happenes when the grab/Release button is called depending on its current state.
        /// </summary>
        public void GrabRelease()
        {
            if (interactionController.isGrabbingObject)
            {
                interactionController.ReleaseGrabbedObject();
            }
            else
            {
                interactionController.GrabObject();
            }
        }
        
        /// <summary>
        /// Adjusts the button labeling, depending on whether or not there is a object currently grabbed or not.
        /// </summary>
        private void AdjustButtonLabel()
        {
            if (interactionController.isGrabbingObject)
            {
                grabButtonText.text = "Release";
            }
            else
            {
                grabButtonText.text = "Grab";
            }
        }

        /// <summary>
        /// Rotates the Grabbed Object via Touch.
        /// </summary>
        private void TouchRotation()
        {
                if (Input.touchCount < 1)
                {
                    return;
                }
                Touch touch = Input.GetTouch(0);
                Quaternion yRotation = Quaternion.Euler(0f, -touch.deltaPosition.x * grabbedObjectRotationSpeed, 0f);
                interactionController.grabber.transform.rotation =
                yRotation * interactionController.grabber.transform.rotation;
        }

    }
}
