using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Shows the expert's picture in its round frame (the parent is a white circle): the picture is cropped to a centred
    /// square and cut round with a smooth edge by the Round Picture shader, so transparent pictures sit on the white circle
    /// and opaque ones are cut round, whatever the trainer uses. The frame's hard stencil mask is switched off (its edge
    /// is jagged).
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class RoundExpertPicture : MonoBehaviour
    {
        private static Shader shader;
        private Image image;
        private Material material;
        private Sprite shown;

        private void Awake()
        {
            image = GetComponent<Image>();

            // the picture fills the frame exactly: the shader crops and cuts it (no envelope fitting, no stencil mask)
            var fitter = GetComponent<AspectRatioFitter>();
            if (fitter != null) fitter.enabled = false;
            var mask = transform.parent != null ? transform.parent.GetComponent<Mask>() : null;
            if (mask != null) mask.enabled = false;
            var rect = (RectTransform)transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;

            if (shader == null) shader = Resources.Load<Shader>("RoundPicture");
            material = new Material(shader);
            image.material = material;
        }

        private void OnDestroy()
        {
            if (material != null) Destroy(material);
        }

        private void LateUpdate()
        {
            if (image.sprite == shown) return;
            shown = image.sprite;
            var crop = new Vector4(0, 0, 1, 1);
            if (shown != null && shown.rect.width > 0 && shown.rect.height > 0)
            {
                var aspect = shown.rect.width / shown.rect.height;
                crop = aspect > 1 ? new Vector4((1 - 1 / aspect) / 2, 0, 1 / aspect, 1)
                    : new Vector4(0, (1 - aspect) / 2, 1, aspect);
            }
            material.SetVector("_CropRect", crop);
        }
    }
}
