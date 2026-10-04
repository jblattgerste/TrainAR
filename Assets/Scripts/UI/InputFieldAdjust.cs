using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Keeps the input question above a keyboard whose bounds are reported by Unity.
    /// Native mobile input remains available when the platform cannot report those bounds.
    /// </summary>
    public class InputFieldAdjust : MonoBehaviour
    {
        [Header("Recttransforms")]
        [SerializeField] private RectTransform inputFieldRect;
        [SerializeField] private RectTransform canvasRect;

        private TMP_InputField inputField;
        private RectTransform inputFieldParent;
        private Canvas canvas;
        private Vector2 inputFieldOriginalPosition;
        private Vector2 appliedOffset;
        private readonly Vector3[] corners = new Vector3[4];
        private bool applicationPaused;
        private int lastAdjustmentFrame = -1;

        private void Awake()
        {
            if (inputFieldRect == null) return;

            inputFieldOriginalPosition = inputFieldRect.anchoredPosition;
            inputFieldParent = inputFieldRect.parent as RectTransform;
            inputField = inputFieldRect.GetComponentInChildren<TMP_InputField>(true);
            canvas = canvasRect != null ? canvasRect.GetComponentInParent<Canvas>() : inputFieldRect.GetComponentInParent<Canvas>();
            if (canvas != null) canvas = canvas.rootCanvas;
        }

        private void LateUpdate()
        {
            AdjustInputFieldPosition();
        }

        /// <summary>
        /// Also called by the input field's Select and Deselect events. LateUpdate follows
        /// keyboard animation and size changes without starting competing coroutines.
        /// </summary>
        public void AdjustInputFieldPosition()
        {
            if (!isActiveAndEnabled || applicationPaused || inputFieldRect == null || inputFieldParent == null ||
                inputField == null || !inputFieldRect.gameObject.activeInHierarchy || !inputField.isFocused ||
                !TouchScreenKeyboard.isSupported || !TouchScreenKeyboard.visible)
            {
                ResetPosition();
                return;
            }

            Rect keyboardArea = TouchScreenKeyboard.area;
            if (Screen.height <= 0 || keyboardArea.width <= 0 || keyboardArea.height <= 0 || keyboardArea.height >= Screen.height ||
                float.IsNaN(keyboardArea.x) || float.IsNaN(keyboardArea.width) ||
                float.IsNaN(keyboardArea.y) || float.IsNaN(keyboardArea.height) ||
                float.IsInfinity(keyboardArea.x) || float.IsInfinity(keyboardArea.width) ||
                float.IsInfinity(keyboardArea.y) || float.IsInfinity(keyboardArea.height))
            {
                // In particular, Android versions before API 30 report a zero rectangle.
                // Do not guess a keyboard height or access Unity's private Android fields.
                ResetPosition();
                return;
            }

            // Android reports top-left screen coordinates; RectTransformUtility uses bottom-left.
            if (Application.platform == RuntimePlatform.Android)
                keyboardArea.y = Screen.height - keyboardArea.yMax;

            Camera canvasCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;

            // Measure the original position without moving the transform twice every frame.
            Vector3 worldOffset = inputFieldParent.TransformVector(new Vector3(appliedOffset.x, appliedOffset.y, 0));
            inputFieldRect.GetWorldCorners(corners);
            Vector2 minimum = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            Vector2 maximum = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            foreach (Vector3 corner in corners)
            {
                Vector2 point = RectTransformUtility.WorldToScreenPoint(canvasCamera, corner - worldOffset);
                minimum = Vector2.Min(minimum, point);
                maximum = Vector2.Max(maximum, point);
            }

            if (keyboardArea.xMax <= minimum.x || keyboardArea.xMin >= maximum.x ||
                keyboardArea.yMax <= minimum.y || keyboardArea.yMin >= maximum.y)
            {
                ResetPosition();
                return;
            }

            float keyboardTop = Mathf.Clamp(keyboardArea.yMax, 0, Screen.height);
            float availableShift = Mathf.Max(0, Screen.safeArea.yMax - maximum.y);
            float shift = Mathf.Min(Mathf.Max(0, keyboardTop - minimum.y), availableShift);
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(inputFieldParent, Vector2.zero, canvasCamera, out Vector2 origin) &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(inputFieldParent, new Vector2(0, shift), canvasCamera, out Vector2 shifted))
            {
                // Visibility can arrive after the native keyboard animation. Ease the
                // measured offset instead of jumping, independently of training time scale.
                if (lastAdjustmentFrame != Time.frameCount)
                {
                    // Opening the native keyboard can stall this first frame. That elapsed
                    // time predates our movement and must not consume its whole transition.
                    float deltaTime = lastAdjustmentFrame < 0
                        ? Mathf.Min(Time.unscaledDeltaTime, 1f / 60f)
                        : Time.unscaledDeltaTime;
                    appliedOffset = SmoothOffset(appliedOffset, shifted - origin, deltaTime);
                    lastAdjustmentFrame = Time.frameCount;
                }
                SetPosition(inputFieldOriginalPosition + appliedOffset);
            }
            else
            {
                ResetPosition();
            }
        }

        private static Vector2 SmoothOffset(Vector2 current, Vector2 target, float deltaTime)
        {
            if (deltaTime <= 0) return current;

            // Reach 95% of the measured displacement in about 0.2 seconds at any frame rate.
            Vector2 next = Vector2.LerpUnclamped(current, target, 1 - Mathf.Exp(-deltaTime / 0.07f));
            return (target - next).sqrMagnitude <= 0.01f ? target : next;
        }

        private void SetPosition(Vector2 position)
        {
            if (inputFieldRect != null && !inputFieldRect.anchoredPosition.Equals(position))
                inputFieldRect.anchoredPosition = position;
        }

        private void ResetPosition()
        {
            SetPosition(inputFieldOriginalPosition);
            appliedOffset = Vector2.zero;
            lastAdjustmentFrame = -1;
        }

        private void OnDisable()
        {
            ResetPosition();
        }

        private void OnApplicationPause(bool paused)
        {
            applicationPaused = paused;
            if (paused) ResetPosition();
        }

        private void OnApplicationFocus(bool focused)
        {
            if (!focused) ResetPosition();
            // Android can lose application focus when its own keyboard opens. LateUpdate
            // follows the field and keyboard state so that event does not disable adjustment.
        }
    }
}
