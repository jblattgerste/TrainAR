using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Keeps the expert's picture filling its round frame (the parent is a white circle with a Mask): the picture
    /// covers the circle in its own proportions, so transparent pictures sit on the white circle and opaque ones are
    /// cut round, whatever the trainer uses.
    /// </summary>
    [RequireComponent(typeof(Image), typeof(AspectRatioFitter))]
    public class RoundExpertPicture : MonoBehaviour
    {
        private Image image;
        private AspectRatioFitter fitter;
        private Sprite shown;

        private void Awake()
        {
            image = GetComponent<Image>();
            fitter = GetComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        }

        private void LateUpdate()
        {
            if (image.sprite == shown) return;
            shown = image.sprite;
            if (shown != null && shown.rect.height > 0)
                fitter.aspectRatio = shown.rect.width / shown.rect.height;
        }
    }
}
