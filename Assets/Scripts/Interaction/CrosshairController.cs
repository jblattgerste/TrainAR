using Others;
using UnityEngine;

namespace Interaction
{
    /// <summary>
    /// The CrosshairController activates/deactivates the crosshair which is positioned in the middle of the screen.
    /// This can e.g. happen when an object is grabbed or an overlay is active.
    /// </summary>
    public class CrosshairController : MonoBehaviour
    {
        /// <summary>
        /// Reference to the RectTransform of the crosshair UI.
        /// </summary>
        /// <value>One crosshair per scene is referenced.</value>
        [SerializeField]
        [Tooltip("Reference to the RectTransform of the crosshair UI.")]
        private RectTransform crosshair;
        
        /// <summary>
        /// Changes depending if the contextButtons are active.
        /// </summary>
        /// <value>True if contextButtons are active.</value>
        [HideInInspector]
        [Tooltip("True if contextButtons are active.")]
        public bool contextButtonsAreActive;
        
        /// <summary>
        /// Disables the crosshair at the start of the scenario.
        /// </summary>
        private void Start()
        {
            DeactivateCrosshair();
        }

        /// <summary>
        /// Adds listener to the prefabWSpawned and RepositionPrefab.
        /// </summary>
        private void OnEnable()
        {
            PrefabSpawningController.prefabSpawned += ActivateCrosshair;
            PrefabSpawningController.RepositionPrefab += DeactivateCrosshair;
        }

        /// <summary>
        /// Remove listener to the prefabSpawned and RepostionPrefab.
        /// </summary>
        private void OnDisable()
        {
            PrefabSpawningController.prefabSpawned -= ActivateCrosshair;
            PrefabSpawningController.RepositionPrefab -= DeactivateCrosshair;
        }

        /// <summary>
        /// Activate the crosshair.
        /// </summary>
        public void ActivateCrosshair()
        {
            crosshair.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Deactivates the crosshair.
        /// </summary>
        public void DeactivateCrosshair()
        {
            crosshair.gameObject.SetActive(false);
        }
        /// <summary>
        /// Deactivates the context buttons.
        /// </summary>
        public void DeactivateContextButtons()
        {
            crosshair.transform.GetChild(1).gameObject.SetActive(false);
            contextButtonsAreActive = false;
        }
    
        /// <summary>
        /// Activates the context buttons.
        /// </summary>
        public void ActivateContextButtons()
        {
            crosshair.transform.GetChild(1).gameObject.SetActive(true);
            contextButtonsAreActive = true;
        }
        
    }
}
