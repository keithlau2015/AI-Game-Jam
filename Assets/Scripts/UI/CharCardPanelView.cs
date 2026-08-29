using Platformer.Mechanics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.UI
{
    public class CharCardPanelView : MonoBehaviour
    {
        public Image portraitImage;
        public Image backgroundImage;
        public Slider happinessSlider;
        public TMP_Text nameText;
        public Image highlightImage;

        public WorkerUnit BoundWorker { get; private set; }

        public void EnsureBindings()
        {
            if (portraitImage == null)
            {
                var portraitTransform = transform.Find("char");
                if (portraitTransform != null)
                    portraitImage = portraitTransform.GetComponent<Image>();
            }

            if (backgroundImage == null)
            {
                var bgTransform = transform.Find("bg");
                if (bgTransform != null)
                    backgroundImage = bgTransform.GetComponent<Image>();
            }

            if (happinessSlider == null)
            {
                var bottom = transform.Find("bottom");
                if (bottom != null)
                    happinessSlider = bottom.GetComponentInChildren<Slider>(true);
            }

            if (nameText == null)
                nameText = GetComponentInChildren<TMP_Text>(true);

            if (highlightImage == null)
            {
                var highlight = transform.Find("Highlight");
                if (highlight != null)
                    highlightImage = highlight.GetComponent<Image>();
            }
        }

        public RectTransform PickRect
        {
            get
            {
                EnsureBindings();
                if (portraitImage != null)
                    return portraitImage.rectTransform;
                return transform as RectTransform;
            }
        }

        public void BindWorker(WorkerUnit worker)
        {
            BoundWorker = worker;
        }

        public bool ContainsScreenPoint(Vector2 screenPoint, Camera eventCamera)
        {
            var rect = PickRect;
            if (rect == null)
                return false;
            return RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint, eventCamera);
        }

        public void Bind(string displayName, Color portraitColor, int happiness, bool highlighted)
        {
            EnsureBindings();

            if (portraitImage != null)
            {
                portraitImage.gameObject.SetActive(true);
                portraitImage.color = portraitColor;
            }

            if (nameText == null)
                nameText = EnsureNameLabel();

            if (nameText != null)
                nameText.text = displayName;

            if (happinessSlider != null)
            {
                happinessSlider.interactable = false;
                happinessSlider.value = Mathf.Clamp01(happiness / 100f);
            }

            if (highlightImage != null)
                highlightImage.enabled = highlighted;
        }

        TMP_Text EnsureNameLabel()
        {
            var labelObject = new GameObject("NameLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            var parent = backgroundImage != null ? backgroundImage.transform : transform;
            labelObject.transform.SetParent(parent, false);
            var rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.08f, 0.55f);
            rect.anchorMax = new Vector2(0.92f, 0.92f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var text = labelObject.GetComponent<TextMeshProUGUI>();
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            return text;
        }
    }
}
