using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Static;
using UnityEngine;

namespace Interaction
{
    /// <summary>
    /// Handles material related functionality for TrainAR objects.
    /// </summary>

    public class MaterialController : MonoBehaviour
    {
        /// <summary>
        /// Reference to the material that is on the object when selected.
        /// </summary>
        /// <value>Default material is referenced.</value>
        [Header("References: ")]
        [SerializeField]
        [Tooltip("Reference to the material that is on the object when selected.")]
        private Material selectionMaterial;
        /// <summary>
        /// Reference to the material that is on the object when overlapped with another TrainAR object.
        /// </summary>
        /// <value>Default material is referenced.</value>
        [SerializeField]
        [Tooltip("Reference to the material that is on the object when overlapped with another TrainAR object.")]
        private Material transparentMaterial;

        /// <summary>
        /// Definition of the error color.
        /// </summary>
        /// <value>Default is Color.red.</value>
        [Header("Options: ")]
        [SerializeField]
        [Tooltip("Definition of the error color.")]
        private Color errorColor = Color.red;
        /// <summary>
        /// Definition of the sucess color.
        /// </summary>
        /// <value>Default is Color.green</value>
        [SerializeField]
        [Tooltip("Definition of the sucess color.")]
        private Color successColor = Color.green;
        /// <summary>
        /// Default color of the highlight outline, used to point the user to an object (e.g. through the Object Helper node).
        /// </summary>
        /// <value>The TrainAR accent amber (#AD8C11), distinct from the selection, error and success colors.</value>
        public static readonly Color DefaultHighlightColor = new Color(0.6784314f, 0.54901963f, 0.06666667f, 1f);
        /// <summary>
        /// Changed when feedbackOutline is active/inactive.
        /// </summary>
        /// <value>True if a feedback outline is active.</value>
        [Tooltip("True if a feedback outline is active.")]
        private bool feedbackOutlineIsActive = false;
        /// <summary>
        /// Reference holder for the outlines.
        /// </summary>
        /// <value>Set on Awake.</value>
        [Tooltip("Reference holder for the outlines.")]
        private Outline[] outlines;
        /// <summary>
        /// The outline color used for selection, which is the color configured on the Outline component.
        /// </summary>
        /// <value>Set on Awake.</value>
        private Color selectionColor;
        /// <summary>
        /// The outline width configured on the Outline component, restored after feedback animations.
        /// </summary>
        /// <value>Set on Awake.</value>
        private float outlineWidth;
        /// <summary>
        /// True while the object is selected.
        /// </summary>
        private bool isSelected = false;
        /// <summary>
        /// True while the object is highlighted.
        /// </summary>
        private bool isHighlighted = false;
        /// <summary>
        /// The color of the currently active highlight.
        /// </summary>
        private Color activeHighlightColor;
        /// <summary>
        /// The state change request during which the highlight was set. The highlight is removed once a later request is accepted.
        /// </summary>
        private int highlightSetDuringRequest;
        /// <summary>
        /// The StatemachineConnector instance that is listened to for accepted state changes.
        /// </summary>
        /// <value>Set on Awake.</value>
        private StatemachineConnector statemachineConnector;
        /// <summary>
        /// Stores the original material.
        /// </summary>
        /// <value>Stored on Start.</value>
        [Tooltip("Stores the original material.")]
        private Dictionary<GameObject, Material> originalMaterials = new Dictionary<GameObject, Material>();

        /// <summary>
        /// Saves the original material and set missing references.
        /// </summary>
        private void Awake()
        {
            saveOriginalMaterial();
            outlines = gameObject.GetComponents<Outline>();
            selectionColor = outlines[0].OutlineColor;
            outlineWidth = outlines[0].OutlineWidth;

            //Listen for the lifetime of the object so a highlight is also removed while the object is invisible
            statemachineConnector = StatemachineConnector.Instance;
            statemachineConnector.TriggerAcceptedStateChange += OnAcceptedStateChange;
        }

        /// <summary>
        /// Stops listening to the StatemachineConnector.
        /// </summary>
        private void OnDestroy()
        {
            statemachineConnector.TriggerAcceptedStateChange -= OnAcceptedStateChange;
        }

        /// <summary>
        /// Coroutines are stopped when the object is deactivated, so a running feedback animation would never finish.
        /// Ends it here instead so the outline doesn't get stuck.
        /// </summary>
        private void OnDisable()
        {
            if (!feedbackOutlineIsActive) return;
            feedbackOutlineIsActive = false;
            ChangeOutLineVisibility(outlineWidth);
            ApplyOutlineState();
        }

        /// <summary>
        /// Adds listener to multiple TrainAR object events to trigger the changes from outlines.
        /// </summary>
        private void Start()
        {
            GetComponent<Interaction.TrainARObject>().OnSelect.AddListener(AddSelectionMaterial);
            GetComponent<Interaction.TrainARObject>().OnSelect.AddListener(ActivateOutlines);
            GetComponent<Interaction.TrainARObject>().OnDeselect.AddListener(DeactivateOutlines);
            GetComponent<Interaction.TrainARObject>().OnDeselect.AddListener(RemoveSelectionMaterial);
            GetComponent<Interaction.TrainARObject>().error.AddListener(ActivateErrorIndicator);
            GetComponent<Interaction.TrainARObject>().OnCombination.AddListener(ActivateSuccessIndicator);
            GetComponent<Interaction.TrainARObject>().OnCombination.AddListener(RemoveSelectionMaterial);
            GetComponent<Interaction.TrainARObject>().OnInteraction.AddListener(ActivateSuccessIndicator);

            //Show a highlight that was set before the object was active for the first time
            ApplyOutlineState();
        }


        /// <summary>
        /// Activates the Outline of the object.
        /// </summary>
        public void ActivateOutlines()
        {
            isSelected = true;
            ApplyOutlineState();
        }

        /// <summary>
        /// Deactivates the Outline of the object.
        /// </summary>
        public void DeactivateOutlines()
        {
            isSelected = false;
            ApplyOutlineState();
        }

        /// <summary>
        /// Highlights the object with an outline to point the user to it, e.g. as the object to use in the current step.
        /// The highlight is removed automatically once the next action is accepted by the statemachine, or manually
        /// by calling this with false. Selection and feedback outlines take precedence while they are active.
        /// </summary>
        /// <param name="highlighted">Whether the object is highlighted.</param>
        /// <param name="color">The highlight color, <see cref="DefaultHighlightColor"/> if null.</param>
        public void SetHighlight(bool highlighted, Color? color = null)
        {
            isHighlighted = highlighted;
            activeHighlightColor = color ?? DefaultHighlightColor;
            highlightSetDuringRequest = StatemachineConnector.Instance.CurrentStateChangeRequest;
            ApplyOutlineState();
        }

        /// <summary>
        /// Updates the outline after a state change, which might have removed the highlight.
        /// </summary>
        /// <param name="accepted">Whether the state change was accepted.</param>
        private void OnAcceptedStateChange(bool accepted)
        {
            if (accepted) ApplyOutlineState();
        }

        /// <summary>
        /// Shows the outline based on the object's state: Feedback overrides selection, which overrides the highlight.
        /// </summary>
        private void ApplyOutlineState()
        {
            //The highlight is removed once an action after the one it was set in is accepted. A highlight set while the
            //accepted action was handled (e.g. by the nodes after "Correct") belongs to the next step and therefore stays.
            if (isHighlighted && StatemachineConnector.Instance.LastAcceptedStateChangeRequest > highlightSetDuringRequest)
            {
                isHighlighted = false;
            }

            //Not initialized yet (object was never active), this is called again on Start
            if (outlines == null) return;

            //A playing feedback animation owns the outline and calls this when it is done
            if (feedbackOutlineIsActive) return;

            if (isSelected)
            {
                SetOutlineColor(selectionColor);
                ToggleOutlines(true);
            }
            else if (isHighlighted)
            {
                SetOutlineColor(activeHighlightColor);
                ToggleOutlines(true);
            }
            else
            {
                ToggleOutlines(false);
            }
        }
    
        /// <summary>
        /// Adds the selection material to the object.
        /// </summary>
        private void AddSelectionMaterial()
        {
            Renderer[] renderers = gameObject.GetComponents<Renderer>();

            foreach (Renderer objectRenderer in renderers)
            {
                var materials = objectRenderer.sharedMaterials.ToList();
                if(!materials.Contains(selectionMaterial))
                    materials.Add(selectionMaterial);

                objectRenderer.materials = materials.ToArray();
            }
        }
    
        /// <summary>
        /// Removes the selection material from the object.
        /// </summary>
        private void RemoveSelectionMaterial(string combinedWith)
        {
            RemoveSelectionMaterial();
        }

        /// <summary>
        /// Removes the selection material from the object.
        /// </summary>
        private void RemoveSelectionMaterial()
        {
            Renderer[] renderers = gameObject.GetComponents<Renderer>();

            foreach (Renderer objectRenderer in renderers)
            {
                var materials = objectRenderer.sharedMaterials.ToList();
                if(materials.Contains(selectionMaterial))
                    materials.Remove(selectionMaterial);
                objectRenderer.materials = materials.ToArray();
            }
        }

        /// <summary>
        /// Replaces all materials on the this Object with a material for combining.
        /// </summary>
        public void ChangeToCombineMaterial()
        {
            //Grabbed object get's combine shader
            Renderer[] objectRenderers = gameObject.GetComponents<Renderer>();

            foreach (Renderer objectRenderer in objectRenderers)
            {
                //Replace the material with the combine material
                objectRenderer.material = transparentMaterial;
            }
        }

        /// <summary>
        /// Saves the original material of this object to restore it later.
        /// </summary>
        private void saveOriginalMaterial()
        {
            Renderer[] objectRenderers = gameObject.GetComponents<Renderer>();
            foreach (Renderer objectRenderer in objectRenderers)
            {
                if (originalMaterials.ContainsKey(objectRenderer.gameObject)) return;
                //Store the reference to the object and its original material to reset them after placement
                originalMaterials.Add(objectRenderer.gameObject, objectRenderer.material);
            }
        }

        /// <summary>
        /// Resets the current materials back to original materials of the object.
        /// </summary>
        public void resetOriginalMaterial()
        {
            foreach (KeyValuePair<GameObject, Material> obj in originalMaterials)
            {
                obj.Key.GetComponent<Renderer>().material = obj.Value;
            }
        }

        /// <summary>
        /// Change the original material of the given object with to a new material.
        /// </summary>
        /// <param name="gameObject">The gameobject where the material is changed.</param>
        /// <param name="newMaterial">The new material.</param>
        public void setNewOriginalMaterial(GameObject gameObject, Material newMaterial)
        {
            originalMaterials[gameObject] = newMaterial;
        }

        /// <summary>
        /// Toggle Outline on and off.
        /// </summary>
        private void ToggleOutlines(bool toggle)
        {
            foreach (var ol in outlines)
            {
                ol.enabled = toggle;
            }
        }

        /// <summary>
        /// Sets the color of all outlines.
        /// </summary>
        /// <param name="color">The new outline color.</param>
        private void SetOutlineColor(Color color)
        {
            foreach (var ol in outlines)
            {
                //Only set on changes, as this is called e.g. every frame while an object is grabbed
                if (ol.OutlineColor != color) ol.OutlineColor = color;
            }
        }
    
        /// <summary>
        /// Make Outline invisible without deactivating it
        /// </summary>
        /// <param name="outlineWidth"></param>
        private void ChangeOutLineVisibility(float outlineWidth)
        {
            foreach (var ol in outlines)
            {
                ol.OutlineWidth = outlineWidth;
            }
        }

        /// <summary>
        /// Play Error outline animation in coroutine.
        /// </summary>
        public void ActivateErrorIndicator()
        {
            if (feedbackOutlineIsActive == true || !this.gameObject.activeInHierarchy) return;
            StartCoroutine(playErrorOutlineSequence());
        }

        /// <summary>
        /// Play the error outline sequence.
        /// </summary>
        /// <returns>IEnumerator for waits.</returns>
        private IEnumerator playErrorOutlineSequence()
        {
        
            //Indicate that the feedback outline animation ist currently playing
            feedbackOutlineIsActive = true;
        
            //Store the original outline width
            float initialOutlineWith = outlines[0].OutlineWidth;
        
            //Change the outline color for the error feedback
            SetOutlineColor(errorColor);

            //----Start the animation
            ToggleOutlines(true);
            ChangeOutLineVisibility(initialOutlineWith);
            yield return new WaitForSeconds(0.3f);
            ChangeOutLineVisibility(0);
            yield return new WaitForSeconds(0.3f);
            ChangeOutLineVisibility(initialOutlineWith);
            yield return new WaitForSeconds(0.3f);
            ChangeOutLineVisibility(0);
            yield return new WaitForSeconds(0.3f);
            ChangeOutLineVisibility(initialOutlineWith);
            yield return new WaitForSeconds(0.3f);
            ChangeOutLineVisibility(0);
            yield return new WaitForSeconds(0.3f);
            ChangeOutLineVisibility(initialOutlineWith);
            yield return new WaitForSeconds(0.3f);
            //----End the animation

            //Hand the outline back, e.g. to show the selection or highlight again
            feedbackOutlineIsActive = false;
            ApplyOutlineState();
        }
    
        /// <summary>
        /// Play Success outline animation in coroutine.
        /// </summary>
        public void ActivateSuccessIndicator()
        {
            if (feedbackOutlineIsActive == true || !this.gameObject.activeInHierarchy) return;
            StartCoroutine(playSuccessOutlineSequence());
        }

        /// <summary>
        /// Play Success outline animation in coroutine.
        /// </summary>
        /// 
        public void ActivateSuccessIndicator(string CombinedWith)
        {
            if (feedbackOutlineIsActive == true || !this.gameObject.activeInHierarchy) return;
            StartCoroutine(playSuccessOutlineSequence());
        }

        /// <summary>
        /// Play the sucess outline sequence.
        /// </summary>
        /// <returns>IEnumerator for waits.</returns>
        private IEnumerator playSuccessOutlineSequence()
        {
        
            //Indicate that the feedback outline animation ist currently playing
            feedbackOutlineIsActive = true;
        
            //Store the original outline width
            float initialOutlineWith = outlines[0].OutlineWidth;
        
            //Change the outline color for the success feedback
            SetOutlineColor(successColor);

            //----Start the animation
            ToggleOutlines(true);
            ChangeOutLineVisibility(initialOutlineWith);
            yield return new WaitForSeconds(0.3f);
            ChangeOutLineVisibility(0);
            yield return new WaitForSeconds(0.3f);
            ChangeOutLineVisibility(initialOutlineWith);
            yield return new WaitForSeconds(0.3f);
            ChangeOutLineVisibility(0);
            yield return new WaitForSeconds(0.3f);
            ChangeOutLineVisibility(initialOutlineWith);
            yield return new WaitForSeconds(0.3f);
            //----End the animation

            //Hand the outline back, e.g. to show the selection or highlight again
            feedbackOutlineIsActive = false;
            ApplyOutlineState();
        }
    }
}

