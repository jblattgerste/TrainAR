using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// A soft shadow behind a UI element (buttons, the insight bubble, the expert). The shadow is a sibling right before
    /// the element that follows its position, size and scale. On buttons, it is only shown while they can be used.
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SoftShadow : MonoBehaviour
    {
        /// <summary>
        /// The blurred shape: a 9-sliced rounded rectangle, or a circle for round elements.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("The blurred shape: a 9-sliced rounded rectangle, or a circle for round elements.")]
        private Sprite shadowSprite;
        /// <summary>
        /// Whether the element is round (the circle shadow grows with the element instead of a fixed blur margin).
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField]
        [Tooltip("Whether the element is round (the circle shadow grows with the element instead of a fixed blur margin).")]
        private bool round;
        /// <summary>
        /// How far the shadow falls below the element.
        /// </summary>
        /// <value>Default is 10.</value>
        [SerializeField]
        [Tooltip("How far the shadow falls below the element.")]
        private float offset = 10f;
        /// <summary>
        /// The darkness of the shadow.
        /// </summary>
        /// <value>Default is 0.35.</value>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("The darkness of the shadow.")]
        private float opacity = 0.35f;

        // the blur margin of the rounded shadow sprite and the circle's share of the round sprite (in sprite pixels)
        private const float Margin = 40f, RoundMargin = 32f, RoundSize = 256f;

        private RectTransform rect, shadow;
        private Image shadowImage;
        private Selectable selectable;

        private void Awake()
        {
            rect = (RectTransform)transform;
            selectable = GetComponent<Selectable>();
        }

        private void OnEnable()
        {
            if (shadow == null) Create();
            shadow.gameObject.SetActive(true);
            LateUpdate();
        }

        private void OnDisable()
        {
            if (shadow != null) shadow.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            if (shadow != null) Destroy(shadow.gameObject);
        }

        private void Create()
        {
            var go = new GameObject(name + " Shadow", typeof(RectTransform));
            go.layer = gameObject.layer;
            shadow = (RectTransform)go.transform;
            shadow.SetParent(rect.parent, false);
            go.AddComponent<LayoutElement>().ignoreLayout = true;
            shadowImage = go.AddComponent<Image>();
            shadowImage.sprite = shadowSprite;
            shadowImage.type = round ? Image.Type.Simple : Image.Type.Sliced;
            shadowImage.color = new Color(0, 0, 0, opacity);
            shadowImage.raycastTarget = false;
        }

        private void LateUpdate()
        {
            if (shadow == null) return;
            if (shadow.GetSiblingIndex() != rect.GetSiblingIndex() - 1)
                shadow.SetSiblingIndex(rect.GetSiblingIndex());
            shadowImage.enabled = selectable == null || selectable.IsInteractable();

            var spread = round ? rect.rect.width * RoundMargin / (RoundSize - 2 * RoundMargin) : Margin;
            shadow.anchorMin = rect.anchorMin;
            shadow.anchorMax = rect.anchorMax;
            shadow.pivot = rect.pivot;
            shadow.localScale = rect.localScale;
            shadow.localRotation = rect.localRotation;
            shadow.sizeDelta = rect.sizeDelta + 2 * spread * Vector2.one;
            shadow.anchoredPosition = rect.anchoredPosition + new Vector2((2 * rect.pivot.x - 1) * spread,
                (2 * rect.pivot.y - 1) * spread - offset);
        }
    }
}
