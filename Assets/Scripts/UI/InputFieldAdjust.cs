using System.Collections;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// InputFieldAdjust Adjusts the UI of the Input field when the keyboard is opened.
    /// This is mostly problematic on Android otherwise.
    /// </summary>
    public class InputFieldAdjust : MonoBehaviour
    {
        /// <summary>
        /// Reference to the inputField RectTransform.
        /// </summary>
        /// <value>Set in inspector.</value>
        [Header("Recttransforms")]
        [SerializeField] private RectTransform inputFieldRect;
        /// <summary>
        /// Reference to the canvas RectTransform.
        /// </summary>
        /// <value>Set in inspector.</value>
        [SerializeField] private RectTransform canvasRect;
        /// <summary>
        /// Stores the original position of the input field.
        /// </summary>
        /// <value>Set on runtime.</value>
        private Vector3 inputFieldOriginalPosition;
    
        /// <summary>
        /// Sets the inputFieldOriginalPosition.
        /// </summary>
        void Awake()
        {
            inputFieldOriginalPosition = inputFieldRect.localPosition;
        }

        /// <summary>
        /// Returns the keyboard height relative to the used canvas.
        /// </summary>
        /// <param name="rectTransform">The Rect Transform of the used canvas</param>
        /// <param name="includeInput">If this is true, the height of the Keyboard + Android input field is returned</param>
        /// <returns></returns>
        private static int GetRelativeKeyboardHeight(RectTransform rectTransform, bool includeInput)
        {
            int keyboardHeight = GetKeyboardHeight(includeInput);
            float screenToRectRatio = Screen.height / rectTransform.rect.height;
            float keyboardHeightRelativeToRect = keyboardHeight / screenToRectRatio;
 
            return (int) keyboardHeightRelativeToRect;
        }

        /// <summary>
        /// Returns the keyboard height in display pixels.
        /// </summary>
        private static int GetKeyboardHeight(bool includeInput)
        {
#if UNITY_ANDROID
            using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var unityPlayer = unityClass.GetStatic<AndroidJavaObject>("currentActivity").Get<AndroidJavaObject>("mUnityPlayer");
                var view = unityPlayer.Call<AndroidJavaObject>("getView");
                var dialog = unityPlayer.Get<AndroidJavaObject>("mSoftInputDialog");
 
                if (view == null || dialog == null)
                    return 0;
 
                var decorHeight = 0;
 
                if (includeInput)
                {
                    var decorView = dialog.Call<AndroidJavaObject>("getWindow").Call<AndroidJavaObject>("getDecorView");
 
                    if (decorView != null)
                        decorHeight = decorView.Call<int>("getHeight");
                }
 
                using (var rect = new AndroidJavaObject("android.graphics.Rect"))
                {
                    view.Call("getWindowVisibleDisplayFrame", rect);
                    return Display.main.systemHeight - rect.Call<int>("height") + decorHeight;
                }
            }
#else
        var height = Mathf.RoundToInt(TouchScreenKeyboard.area.height);
        return height >= Display.main.systemHeight ? 0 : height;
#endif
        }


        /// <summary>
        /// Coroutine to adjust the position of Inputfield UI. 
        /// </summary>
        private IEnumerator AdjustInputField()
        {
            float currentHeight = GetRelativeKeyboardHeight(canvasRect, true);
            yield return new WaitForSeconds(0.2f);
        
            // The Android keyboards size changes dynamically, so this checks whether its done moving to it's maximum size.
            while (GetRelativeKeyboardHeight(canvasRect, true) != currentHeight)
            {
                yield return new WaitForSeconds(0.2f);
                currentHeight = GetRelativeKeyboardHeight(canvasRect, true);
                yield return null;
            }
            var localPosition = inputFieldRect.localPosition;
            localPosition = new Vector3(localPosition.x,inputFieldOriginalPosition.y+currentHeight, localPosition.z);
            inputFieldRect.localPosition = localPosition;
        }


        /// <summary>
        /// Adjusts the position of the Inputfield UI when the Android or iOS keyboard is opened.
        /// </summary>
        public void AdjustInputFieldPosition()
        { 
            StartCoroutine(AdjustInputField());
        }
    }
}
