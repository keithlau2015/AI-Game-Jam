using System.Collections;
using System.Collections.Generic;
using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Mechanics;
using Platformer.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Platformer.Core.Simulation;

namespace Platformer.UI
{
    public class RandomEventUIController : MonoBehaviour
    {
        public Canvas targetCanvas;
        public TMP_FontAsset font;

        GameObject panelRoot;
        TMP_Text titleText;
        TMP_Text descriptionText;
        TMP_Text outcomeText;
        readonly List<Button> optionButtons = new List<Button>();
        GameObject buttonRow;

        void Awake()
        {
            EnsureUI();
            HidePanel();
            RandomEventTriggered.OnExecute += OnEventTriggered;
            RandomEventResolved.OnExecute += OnEventResolved;
        }

        void OnDestroy()
        {
            RandomEventTriggered.OnExecute -= OnEventTriggered;
            RandomEventResolved.OnExecute -= OnEventResolved;
        }

        void OnEventTriggered(RandomEventTriggered triggered)
        {
            ShowEvent(triggered.definition);
        }

        void OnEventResolved(RandomEventResolved resolved)
        {
            var outcome = resolved.definition.options[resolved.optionIndex].outcomeText;
            if (!string.IsNullOrEmpty(outcome))
            {
                outcomeText.text = outcome;
                outcomeText.gameObject.SetActive(true);
                StartCoroutine(HideAfterDelay(2f));
            }
            else
            {
                HidePanel();
            }
        }

        IEnumerator HideAfterDelay(float seconds)
        {
            yield return new WaitForSecondsRealtime(seconds);
            HidePanel();
        }

        void ShowEvent(RandomEventDefinition definition)
        {
            if (definition == null)
                return;

            EnsureUI();
            panelRoot.SetActive(true);
            titleText.text = definition.title;
            descriptionText.text = definition.description;
            outcomeText.gameObject.SetActive(false);
            outcomeText.text = string.Empty;
            RebuildButtons(definition);
        }

        void HidePanel()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
        }

        void RebuildButtons(RandomEventDefinition definition)
        {
            ClearButtons();

            for (var i = 0; i < definition.options.Length; i++)
            {
                var index = i;
                var option = definition.options[i];
                var buttonObject = CreateButton(option.label);
                var button = buttonObject.GetComponent<Button>();
                button.onClick.AddListener(() => ChooseOption(definition, index));
                optionButtons.Add(button);
            }
        }

        void ChooseOption(RandomEventDefinition definition, int optionIndex)
        {
            foreach (var button in optionButtons)
                button.interactable = false;

            var ev = Schedule<RandomEventResolved>();
            ev.definition = definition;
            ev.optionIndex = optionIndex;

            var outcome = definition.options[optionIndex].outcomeText;
            if (string.IsNullOrEmpty(outcome))
                HidePanel();
        }

        void ClearButtons()
        {
            foreach (var button in optionButtons)
            {
                if (button != null)
                    Destroy(button.gameObject);
            }
            optionButtons.Clear();
        }

        void EnsureUI()
        {
            if (panelRoot != null)
                return;

            var canvas = targetCanvas;
            if (canvas == null)
            {
                var canvasObject = new GameObject("RandomEventCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 200;
                var scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }

            panelRoot = new GameObject("RandomEventPanel", typeof(RectTransform), typeof(Image));
            panelRoot.transform.SetParent(canvas.transform, false);
            var panelImage = panelRoot.GetComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.82f);

            var panelRect = panelRoot.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var card = CreateCard(panelRoot.transform);
            titleText = CreateText(card, 42, FontStyles.Bold, TextAlignmentOptions.Center);
            descriptionText = CreateText(card, 28, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            outcomeText = CreateText(card, 24, FontStyles.Italic, TextAlignmentOptions.Center);
            outcomeText.color = new Color(0.85f, 0.95f, 1f, 1f);

            buttonRow = new GameObject("OptionRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            buttonRow.transform.SetParent(card, false);
            var rowRect = buttonRow.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0.05f, 0.05f);
            rowRect.anchorMax = new Vector2(0.95f, 0.22f);
            rowRect.offsetMin = Vector2.zero;
            rowRect.offsetMax = Vector2.zero;
            var layout = buttonRow.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
        }

        RectTransform CreateCard(Transform parent)
        {
            var cardObject = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            cardObject.transform.SetParent(parent, false);
            var cardRect = cardObject.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.1f, 0.15f);
            cardRect.anchorMax = new Vector2(0.9f, 0.85f);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;
            cardObject.GetComponent<Image>().color = new Color(0.12f, 0.12f, 0.16f, 0.96f);
            return cardRect;
        }

        TMP_Text CreateText(Transform parent, float fontSize, FontStyles style, TextAlignmentOptions alignment)
        {
            var textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            var text = textObject.GetComponent<TextMeshProUGUI>();
            if (font != null)
                text.font = font;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = alignment;
            text.color = Color.white;
            text.textWrappingMode = TextWrappingModes.Normal;
            var layoutElement = textObject.AddComponent<LayoutElement>();
            layoutElement.minHeight = fontSize * 2f;
            layoutElement.flexibleHeight = 1f;
            return text;
        }

        GameObject CreateButton(string label)
        {
            var buttonObject = new GameObject("OptionButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(buttonRow.transform, false);
            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.22f, 0.45f, 0.78f, 1f);

            var textObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(buttonObject.transform, false);
            var text = textObject.GetComponent<TextMeshProUGUI>();
            if (font != null)
                text.font = font;
            text.text = label;
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            var textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(12f, 8f);
            textRect.offsetMax = new Vector2(-12f, -8f);

            return buttonObject;
        }
    }
}
