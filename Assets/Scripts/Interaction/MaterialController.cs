using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        }


        /// <summary>
        /// Activates the Outline of the object.
        /// </summary>
        public void ActivateOutlines()
        {
            ToggleOutlines(true);
        }

        /// <summary>
        /// Deactivates the Outline of the object.
        /// </summary>
        public void DeactivateOutlines()
        {
            if (feedbackOutlineIsActive == true )
            {
                StartCoroutine(ToggleOutlinesDelayed(false));
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
        /// Toggle the outline delayed if an outline animation is currently playing.
        /// </summary>
        /// <param name="toggle"></param>
        /// <returns></returns>
        private IEnumerator ToggleOutlinesDelayed(bool toggle)
        {
            yield return new WaitUntil(() => feedbackOutlineIsActive == false);
            ToggleOutlines(toggle);
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
        
            //Store the original outline colors
            Color[] initialColors = new Color[outlines.Length];
            for (int i = 0; i < initialColors.Length; i++)
            {
                initialColors[i] = outlines[i].OutlineColor;
            }

            //Store the original outline width
            float initialOutlineWith = outlines[0].OutlineWidth;
        
            //Change the outline color for the error feedback
            foreach (var ol in outlines)
            {
                ol.OutlineColor = errorColor;
            }

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

            //Restore the original outline colors
            for (int i = 0; i < initialColors.Length; i++)
            {
                outlines[i].OutlineColor = initialColors[i];
            }

            yield return new WaitForSeconds(0.01f);
            ToggleOutlines(false);

            feedbackOutlineIsActive = false;
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
        
            //Store the original outline colors
            Color[] initialColors = new Color[outlines.Length];
            for (int i = 0; i < initialColors.Length; i++)
            {
                initialColors[i] = outlines[i].OutlineColor;
            }
        
            //Store the original outline width
            float initialOutlineWith = outlines[0].OutlineWidth;
        
            //Change the outline color for the success feedback
            foreach (var ol in outlines)
            {
                ol.OutlineColor = successColor;
            }

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

            //Restore the original outline colors
            for (int i = 0; i < initialColors.Length; i++)
            {
                outlines[i].OutlineColor = initialColors[i];
            }
            yield return new WaitForSeconds(0.01f);
            ToggleOutlines(false);
            feedbackOutlineIsActive = false;
        }
    }
}

