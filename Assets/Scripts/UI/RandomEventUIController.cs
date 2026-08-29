using System.Collections.Generic;
using Platformer.Core;
using Platformer.Gameplay;
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

        GameObject choicePanelRoot;
        TMP_Text titleText;
        TMP_Text descriptionText;
        readonly List<Button> optionButtons = new List<Button>();
        GameObject buttonRow;

        GameObject resultPanelRoot;
        TMP_Text resultTitleText;
        TMP_Text resultStatsText;
        TMP_Text resultDescriptionText;
        Button confirmButton;

        SessionModel session;

        void Awake()
        {
            session = Simulation.GetModel<SessionModel>();
            ResolveFont();
            EnsureUI();
            HideAll();
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
            ShowChoice(triggered.definition);
        }

        void OnEventResolved(RandomEventResolved resolved)
        {
            if (resolved.definition == null
                || resolved.definition.options == null
                || resolved.optionIndex < 0
                || resolved.optionIndex >= resolved.definition.options.Length)
            {
                HideAll();
                return;
            }

            ShowResult(resolved.definition.options[resolved.optionIndex]);
        }

        void ShowChoice(RandomEventDefinition definition)
        {
            if (definition == null)
                return;

            EnsureUI();
            HideAll();
            choicePanelRoot.SetActive(true);
            titleText.text = definition.title;
            descriptionText.text = definition.description;
            RebuildButtons(definition);
        }

        void ShowResult(RandomEventOption option)
        {
            EnsureUI();
            choicePanelRoot.SetActive(false);
            ClearButtons();
            resultPanelRoot.SetActive(true);
            resultTitleText.text = "結果";
            resultStatsText.text = RandomEventEffectPresentation.BuildResultLabel(option);
            resultDescriptionText.text = string.IsNullOrEmpty(option.outcomeText)
                ? "這個選擇悄悄地發生了。"
                : option.outcomeText;
        }

        void ResolveFont()
        {
            font = UIFontProvider.Primary;
        }

        void HideAll()
        {
            if (choicePanelRoot != null)
                choicePanelRoot.SetActive(false);
            if (resultPanelRoot != null)
                resultPanelRoot.SetActive(false);
        }

        void RebuildButtons(RandomEventDefinition definition)
        {
            ClearButtons();

            for (var i = 0; i < definition.options.Length; i++)
            {
                var index = i;
                var option = definition.options[i];
                var preview = RandomEventEffectPresentation.BuildPreviewLabel(option);
                var buttonObject = CreateOptionButton(option.label, preview);
                var button = buttonObject.GetComponent<Button>();
                button.onClick.AddListener(() => ChooseOption(definition, index));
                optionButtons.Add(button);
            }
        }

        void ChooseOption(RandomEventDefinition definition, int optionIndex)
        {
            choicePanelRoot.SetActive(false);
            ClearButtons();

            var ev = Schedule<RandomEventResolved>();
            ev.definition = definition;
            ev.optionIndex = optionIndex;
        }

        void AcknowledgeResult()
        {
            HideAll();
            RandomEventFlow.CompleteOutcomeAcknowledgement(session);
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
            if (choicePanelRoot != null)
                return;

            var canvas = EnsureCanvas();
            choicePanelRoot = CreateOverlayPanel(canvas.transform, "RandomEventChoicePanel");
            var choiceCard = CreateCard(choicePanelRoot.transform);
            titleText = CreateText(choiceCard, 42, FontStyles.Bold, TextAlignmentOptions.Center);
            descriptionText = CreateText(choiceCard, 28, FontStyles.Normal, TextAlignmentOptions.TopLeft);

            buttonRow = new GameObject("OptionRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            buttonRow.transform.SetParent(choiceCard, false);
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

            resultPanelRoot = CreateOverlayPanel(canvas.transform, "RandomEventResultPanel");
            var resultCard = CreateCard(resultPanelRoot.transform);
            resultTitleText = CreateText(resultCard, 38, FontStyles.Bold, TextAlignmentOptions.Center);
            resultStatsText = CreateText(resultCard, 30, FontStyles.Bold, TextAlignmentOptions.Center);
            resultStatsText.color = new Color(0.95f, 0.85f, 0.45f, 1f);
            resultDescriptionText = CreateText(resultCard, 26, FontStyles.Normal, TextAlignmentOptions.TopLeft);

            var confirmRow = new GameObject("ConfirmRow", typeof(RectTransform));
            confirmRow.transform.SetParent(resultCard, false);
            var confirmRect = confirmRow.GetComponent<RectTransform>();
            confirmRect.anchorMin = new Vector2(0.2f, 0.05f);
            confirmRect.anchorMax = new Vector2(0.8f, 0.18f);
            confirmRect.offsetMin = Vector2.zero;
            confirmRect.offsetMax = Vector2.zero;

            var confirmObject = new GameObject("ConfirmButton", typeof(RectTransform), typeof(Image), typeof(Button));
            confirmObject.transform.SetParent(confirmRow.transform, false);
            var confirmImage = confirmObject.GetComponent<Image>();
            confirmImage.color = new Color(0.22f, 0.55f, 0.35f, 1f);
            var confirmRectTransform = confirmObject.GetComponent<RectTransform>();
            confirmRectTransform.anchorMin = Vector2.zero;
            confirmRectTransform.anchorMax = Vector2.one;
            confirmRectTransform.offsetMin = Vector2.zero;
            confirmRectTransform.offsetMax = Vector2.zero;
            confirmButton = confirmObject.GetComponent<Button>();
            confirmButton.onClick.AddListener(AcknowledgeResult);

            var confirmLabelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            confirmLabelObject.transform.SetParent(confirmObject.transform, false);
            var confirmLabelRect = confirmLabelObject.GetComponent<RectTransform>();
            confirmLabelRect.anchorMin = Vector2.zero;
            confirmLabelRect.anchorMax = Vector2.one;
            confirmLabelRect.offsetMin = Vector2.zero;
            confirmLabelRect.offsetMax = Vector2.zero;
            var confirmLabel = confirmLabelObject.GetComponent<TextMeshProUGUI>();
            UIFontProvider.Apply(confirmLabel);
            confirmLabel.text = "確認";
            confirmLabel.fontSize = 26;
            confirmLabel.alignment = TextAlignmentOptions.Center;
            confirmLabel.color = Color.white;
        }

        Canvas EnsureCanvas()
        {
            if (targetCanvas != null)
                return targetCanvas;

            var hud = GameplayHUDView.Instance;
            if (hud != null)
                targetCanvas = hud.GetComponentInParent<Canvas>();

            if (targetCanvas == null)
                targetCanvas = FindFirstObjectByType<Canvas>();

            if (targetCanvas == null)
            {
                var canvasObject = new GameObject("RandomEventCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                targetCanvas = canvasObject.GetComponent<Canvas>();
                targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                targetCanvas.sortingOrder = 200;
                var scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }

            return targetCanvas;
        }

        GameObject CreateOverlayPanel(Transform parent, string name)
        {
            var panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            panel.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.82f);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            panel.SetActive(false);
            return panel;
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
            UIFontProvider.Apply(text);
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

        GameObject CreateOptionButton(string label, string affectedStatsText)
        {
            var buttonObject = new GameObject("OptionButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(VerticalLayoutGroup));
            buttonObject.transform.SetParent(buttonRow.transform, false);
            var image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.22f, 0.45f, 0.78f, 1f);
            var layout = buttonObject.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 10, 10);
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);
            var labelText = labelObject.GetComponent<TextMeshProUGUI>();
            UIFontProvider.Apply(labelText);
            labelText.text = label;
            labelText.fontSize = 24;
            labelText.alignment = TextAlignmentOptions.Center;
            labelText.color = Color.white;

            if (!string.IsNullOrEmpty(affectedStatsText))
            {
                var hintObject = new GameObject("AffectedStats", typeof(RectTransform), typeof(TextMeshProUGUI));
                hintObject.transform.SetParent(buttonObject.transform, false);
                var hintText = hintObject.GetComponent<TextMeshProUGUI>();
                UIFontProvider.Apply(hintText);
                hintText.text = affectedStatsText;
                hintText.fontSize = 18;
                hintText.alignment = TextAlignmentOptions.Center;
                hintText.color = new Color(0.82f, 0.9f, 1f, 1f);
            }

            return buttonObject;
        }
    }
}
