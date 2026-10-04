using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Others
{
    /// <summary>
    /// Owns a single touch from its beginning to its end. A gesture that starts on UI
    /// never becomes a world gesture, even if it subsequently leaves the UI.
    /// </summary>
    internal sealed class WorldTouchGesture
    {
        private readonly List<RaycastResult> raycastResults = new List<RaycastResult>();
        private PointerEventData pointerEvent;
        private EventSystem eventSystem;
        private Finger finger;
        private int touchId;
        private bool blockedByUI;
        private bool enabled;

        public void Enable()
        {
            if (enabled) return;
            EnhancedTouchSupport.Enable();
            enabled = true;
            Reset();
        }

        public void Disable()
        {
            Reset();
            if (!enabled) return;
            EnhancedTouchSupport.Disable();
            enabled = false;
        }

        public void Reset()
        {
            finger = null;
            touchId = 0;
            blockedByUI = false;
        }

        public bool TryGetTouch(out Touch touch)
        {
            touch = default;
            if (!enabled) return false;

            var touches = Touch.activeTouches;
            if (finger != null)
            {
                foreach (var candidate in touches)
                {
                    if (candidate.finger != finger || candidate.touchId != touchId) continue;
                    if (candidate.phase == TouchPhase.Ended || candidate.phase == TouchPhase.Canceled)
                    {
                        Reset();
                        break;
                    }

                    // A modal may have opened over a contact already in progress.
                    // Once UI owns the position, require a fresh world gesture.
                    if (!blockedByUI) blockedByUI = IsOverUI(candidate.screenPosition);
                    touch = candidate;
                    return !blockedByUI;
                }

                // A removed device or lost focus can end a contact without another
                // frame containing it. Never hand that gesture to a held second finger.
                Reset();
            }

            foreach (var candidate in touches)
            {
                if (candidate.phase != TouchPhase.Began) continue;
                finger = candidate.finger;
                touchId = candidate.touchId;
                blockedByUI = IsOverUI(candidate.screenPosition);
                touch = candidate;
                return !blockedByUI;
            }

            return false;
        }

        private bool IsOverUI(Vector2 position)
        {
            if (EventSystem.current == null) return false;
            if (eventSystem != EventSystem.current)
            {
                eventSystem = EventSystem.current;
                pointerEvent = new PointerEventData(eventSystem);
            }

            // Raycast the current position directly: the input module may not have
            // processed this frame yet when a MonoBehaviour's Update runs.
            pointerEvent.Reset();
            pointerEvent.position = position;
            raycastResults.Clear();
            eventSystem.RaycastAll(pointerEvent, raycastResults);
            foreach (var result in raycastResults)
            {
                if (result.module is UnityEngine.UI.GraphicRaycaster) return true;
            }
            return false;
        }
    }
}
