using UnityEngine;

namespace UI
{
    /// <summary>
    /// The SafeArea scripts moves UI components when they would otherwise be occluded by e.g. cutout cameras or speakers
    /// on some newer smartphone devices. 
    /// </summary>
    public class SaveArea : MonoBehaviour
    {
        /// <summary>
        /// Reference to the RectTransform ot the SaveArea.
        /// </summary>
        /// <value>Set on runtime.</value>
        RectTransform Panel;
        /// <summary>
        /// Holds the dimenstions of lastSafeArea.
        /// </summary>
        /// <value>Set on runtime.</value>
        Rect LastSafeArea = new Rect(0, 0, 0, 0);
        /// <summary>
        /// Sets missing references.
        /// </summary>
        void Awake()
        {
            //Only apply the save area if the option for it is set
            if (true)
            {
                Panel = GetComponent<RectTransform>();
                Refresh();
            }
        }
        /// <summary>
        /// Refreshes the safeArea dimenstions.
        /// </summary>
        private void Refresh()
        {
            Rect safeArea = GetSafeArea();

            if (safeArea != LastSafeArea)
                ApplySafeArea(safeArea);
        }
        /// <summary>
        /// Get the current safeArea.
        /// </summary>
        /// <returns>Rect of safeArea.</returns>
        private Rect GetSafeArea()
        {
            return Screen.safeArea;
        }
        /// <summary>
        /// Applys the saveArea.
        /// </summary>
        /// <param name="r">Rect of the SafeArea.</param>
        private void ApplySafeArea(Rect r)
        {
            LastSafeArea = r;
            // Convert safe area rectangle from absolute pixels to normalised anchor coordinates
            Vector2 anchorMin = r.position;
            Vector2 anchorMax = r.position + r.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            Panel.anchorMin = anchorMin;
            Panel.anchorMax = anchorMax;
        }
    }
}
