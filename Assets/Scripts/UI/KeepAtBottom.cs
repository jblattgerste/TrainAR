using UnityEngine;

namespace UI
{
    /// <summary>
    /// Keeps a button's lower edge a fixed distance above the bottom of the safe area, on every screen length. With the
    /// distance equal to the button row's side margin, the buttons sit symmetrically in the lower corners.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class KeepAtBottom : MonoBehaviour
    {
        /// <summary>
        /// The distance from the bottom of the safe area to the button's lower edge (canvas units).
        /// </summary>
        /// <value>Default is 190, the side margin of the interaction buttons.</value>
        [SerializeField]
        [Tooltip("The distance from the bottom of the safe area to the button's lower edge (canvas units).")]
        private float margin = 190f;

        private RectTransform rect;
        private Canvas canvas;

        private void Awake()
        {
            rect = (RectTransform)transform;
            canvas = GetComponentInParent<Canvas>().rootCanvas;
        }

        private void LateUpdate()
        {
            // the target height in world space: the safe area's bottom plus the margin
            var canvasRect = (RectTransform)canvas.transform;
            var corners = new Vector3[4];
            canvasRect.GetWorldCorners(corners);
            var worldPerUnit = (corners[1].y - corners[0].y) / canvasRect.rect.height;
            var safeBottom = Screen.safeArea.yMin / Screen.height * canvasRect.rect.height;
            var target = corners[0].y + (safeBottom + margin) * worldPerUnit;

            // the lower edge without the button's own scale (pop animations must not move it)
            var parent = (RectTransform)rect.parent;
            var targetInParent = parent.InverseTransformPoint(new Vector3(0, target, 0)).y;
            var lowerEdge = rect.localPosition.y + rect.rect.yMin;
            var delta = targetInParent - lowerEdge;
            if (Mathf.Abs(delta) > 0.01f) rect.localPosition += new Vector3(0, delta, 0);
        }
    }
}
