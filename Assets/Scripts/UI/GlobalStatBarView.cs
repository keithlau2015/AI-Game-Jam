using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.UI
{
    public class GlobalStatBarView
    {
        public TMP_Text labelText;
        public Image fillImage;
        public RectTransform fillRect;
        public TMP_Text valueText;

        public void Set(string label, int value, int min, int max, Color fillColor)
        {
            if (labelText != null)
                labelText.text = label;
            if (valueText != null)
                valueText.text = value.ToString();

            var range = Mathf.Max(1, max - min);
            var normalized = Mathf.Clamp01((value - min) / (float)range);
            if (fillRect != null)
                fillRect.anchorMax = new Vector2(normalized, 1f);
            if (fillImage != null)
                fillImage.color = fillColor;
        }
    }
}
