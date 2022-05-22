using Interaction;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Controls the dynamic transformations of the crosshair. It activates and deactivates the crosshair
    /// but also visualizes the distance to a grabbable TrainAR object by converging the crosshairs size until
    /// the object would be selected (e.g. if the user is too far away from an object to grab it).
    /// </summary>
    public class CrosshairOnOff : MonoBehaviour
    {
        /// <summary>
        /// Reference to the Interaction Controller.
        /// </summary>
        /// <value>Once per scene.</value>
        [SerializeField]
        [Tooltip("Reference to the Interaction Controller")]
        private InteractionController interactionController;
        /// <summary>
        /// The Gameobject holding the UI-Elements for the cursor of the crosshair.
        /// </summary>
        /// <value>Once per scene.</value>
        [SerializeField]
        [Tooltip("The Gameobject holding the UI-Elements for the cursor of the crosshair.")]
        private GameObject cursor;
        /// <summary>
        /// The Gameobject holding the UI-Elements for the inner cursor of the crosshair.
        /// </summary>
        /// <value>Once per scene.</value>
        [SerializeField]
        [Tooltip("The Gameobject holding the UI-Elements for the inner cursor of the crosshair.")]
        private GameObject innerCursor;
        /// <summary>
        /// Reference to the RectTransform of the cursor.
        /// </summary>
        /// <value>Set on runtime.</value>
        RectTransform cursor_rt;
        /// <summary>
        /// Sets reference for the interactionController and the cursor_rt.
        /// </summary>
        private void Start()
        {
            interactionController = interactionController.GetComponent<InteractionController>();
            cursor_rt =   cursor.GetComponent<RectTransform>();
        }
        /// <summary>
        /// Enable/Disables the cursor depending if a object is grabbed.
        /// </summary>
        private void Update()
        {
            if (interactionController.isGrabbingObject)
            {
                cursor.SetActive(false);
            }
            else
            {
                cursor.SetActive(true);
                cursor_rt.sizeDelta = GetNewSize();
            }
        }

        /// <summary>
        /// Returns the Vector of the new size of the cursor, depending on the distance of the object hit by the raycast.
        /// </summary>
        /// <returns>The Vector containing the new size of the cursor.</returns>
        private Vector2 GetNewSize()
        {
            float distanceHit = interactionController.hit.distance;
            int sizeMax = 85;
            int sizeMin = 30;
        
            if(distanceHit>1) distanceHit = 1;

            float x=(sizeMin*(1-distanceHit))+(distanceHit*sizeMax);

            return new Vector2(x,x);
        }
    }
}
