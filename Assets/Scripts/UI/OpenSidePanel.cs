using UnityEngine;

namespace UI
{
    /// <summary>
    /// Handles opening or closing of the side panel (burger menu) wiht options for help, quitting and replacing the assembly.
    /// </summary>
    public class OpenSidePanel : MonoBehaviour
    {
        /// <summary>
        /// The Gameobject holding the UI-Panel for the Sidepanel.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("The Gameobject holding the UI-Panel for the Sidepanel.")]
        private GameObject panel;

        /// <summary>
        /// Opens or closes the Panel.
        /// </summary>
        public void TogglePanel()
        {
            if (panel != null)
            {
                Animator animator = panel.GetComponent<Animator>();
                Animator hamburgerToExit = GetComponent<Animator>();
                if (animator != null)
                {
                    bool isOpen = animator.GetBool("open");
                    animator.SetBool("open", !isOpen);
                    hamburgerToExit.SetBool("Pressed", !isOpen);
                }
            }
        }
    }
}
