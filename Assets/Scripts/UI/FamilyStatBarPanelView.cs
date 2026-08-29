using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.UI
{
    public class FamilyStatBarPanelView : MonoBehaviour
    {
        public Image iconImage;
        public TMP_Text labelText;
        public Slider progressSlider;
        public TMP_Text valueText;

        public void EnsureBindings()
        {
            if (iconImage == null)
            {
                var iconTransform = transform.Find("icon");
                if (iconTransform != null)
                    iconImage = iconTransform.GetComponent<Image>();
            }

            if (progressSlider == null)
                progressSlider = GetComponentInChildren<Slider>(true);

            var texts = GetComponentsInChildren<TMP_Text>(true);
            if (labelText == null && texts.Length > 0)
                labelText = texts[0];
            if (valueText == null && texts.Length > 1)
                valueText = texts[1];
        }

        public void SetLabel(string label)
        {
            EnsureBindings();
            if (labelText != null)
                labelText.text = label;
        }

        public void Set(int value, int min, int max)
        {
            EnsureBindings();
            if (valueText != null)
                valueText.text = value.ToString();

            if (progressSlider != null)
            {
                progressSlider.interactable = false;
                var range = Mathf.Max(1, max - min);
                progressSlider.value = Mathf.Clamp01((value - min) / (float)range);
            }
        }
    }
}
