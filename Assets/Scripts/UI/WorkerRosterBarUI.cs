using System.Collections.Generic;
using Platformer.Mechanics;
using Platformer.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.UI
{
    public class WorkerRosterBarUI : MonoBehaviour
    {
        public float barHeight = 140f;

        static Sprite whiteSprite;
        RectTransform barRoot;
        readonly List<CardBinding> cards = new List<CardBinding>();

        struct CardBinding
        {
            public WorkerUnit worker;
            public Image portraitImage;
            public TMP_Text nameText;
            public TMP_Text roleText;
            public Image happinessFill;
            public TMP_Text happinessValueText;
            public Image highlightImage;
        }

        void Awake()
        {
            EnsureBar();
            RefreshCards();
        }

        void LateUpdate()
        {
            RefreshCards();
            UpdateHighlights();
        }

        void EnsureBar()
        {
            if (barRoot != null)
                return;

            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
                return;

            var barObject = new GameObject("WorkerRosterBar", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            barObject.transform.SetParent(canvas.transform, false);
            barRoot = barObject.GetComponent<RectTransform>();
            barRoot.anchorMin = new Vector2(0f, 0f);
            barRoot.anchorMax = new Vector2(1f, 0f);
            barRoot.pivot = new Vector2(0.5f, 0f);
            barRoot.sizeDelta = new Vector2(0f, barHeight);
            barRoot.anchoredPosition = Vector2.zero;

            var backdrop = barObject.GetComponent<Image>();
            backdrop.color = new Color(0.05f, 0.06f, 0.1f, 0.92f);

            var layout = barObject.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 10, 10);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
        }

        void RefreshCards()
        {
            if (barRoot == null)
                return;

            var workers = FindObjectsByType<WorkerUnit>();
            System.Array.Sort(workers, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));

            while (cards.Count < workers.Length)
                cards.Add(CreateCard(barRoot));

            for (var i = 0; i < cards.Count; i++)
            {
                var visible = i < workers.Length;
                cards[i].portraitImage.transform.parent.gameObject.SetActive(visible);
                if (!visible)
                    continue;

                var card = cards[i];
                BindCard(ref card, workers[i]);
                cards[i] = card;
            }
        }

        void BindCard(ref CardBinding card, WorkerUnit worker)
        {
            card.worker = worker;
            card.portraitImage.color = worker.GetComponent<SpriteRenderer>() != null
                ? worker.GetComponent<SpriteRenderer>().color
                : Color.white;
            card.nameText.text = worker.displayName;
            card.roleText.text = worker.role.ToString();

            var happiness = Mathf.Clamp(worker.attributes.happiness, 0, 100);
            card.happinessFill.fillAmount = happiness / 100f;
            card.happinessValueText.text = happiness.ToString();
        }

        void UpdateHighlights()
        {
            for (var i = 0; i < cards.Count; i++)
            {
                if (cards[i].highlightImage == null)
                    continue;

                var active = cards[i].worker != null
                    && (cards[i].worker.state == WorkerState.Dragging || cards[i].worker.state == WorkerState.Working);
                cards[i].highlightImage.enabled = active;
            }
        }

        CardBinding CreateCard(Transform parent)
        {
            var cardObject = new GameObject("RosterCard", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            cardObject.transform.SetParent(parent, false);
            cardObject.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.2f, 0.98f);

            var layout = cardObject.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 8, 8);
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var highlightObject = new GameObject("Highlight", typeof(RectTransform), typeof(Image));
            highlightObject.transform.SetParent(cardObject.transform, false);
            var highlightRect = highlightObject.GetComponent<RectTransform>();
            highlightRect.anchorMin = Vector2.zero;
            highlightRect.anchorMax = Vector2.one;
            highlightRect.offsetMin = Vector2.zero;
            highlightRect.offsetMax = Vector2.zero;
            var highlight = highlightObject.GetComponent<Image>();
            highlight.color = new Color(0.95f, 0.78f, 0.25f, 0.35f);
            highlight.enabled = false;

            var portraitObject = new GameObject("Portrait", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            portraitObject.transform.SetParent(cardObject.transform, false);
            portraitObject.GetComponent<LayoutElement>().minHeight = 42f;
            portraitObject.GetComponent<LayoutElement>().preferredHeight = 42f;
            var portraitImage = portraitObject.GetComponent<Image>();
            portraitImage.sprite = EnsureWhiteSprite();

            var nameText = CreateText(cardObject.transform, 20, FontStyles.Bold);
            var roleText = CreateText(cardObject.transform, 16, FontStyles.Italic);
            var happinessRow = CreateHappinessRow(cardObject.transform);

            return new CardBinding
            {
                portraitImage = portraitImage,
                nameText = nameText,
                roleText = roleText,
                happinessFill = happinessRow.fill,
                happinessValueText = happinessRow.valueText,
                highlightImage = highlight
            };
        }

        (Image fill, TMP_Text valueText) CreateHappinessRow(Transform parent)
        {
            var rowObject = new GameObject("HappinessRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            rowObject.transform.SetParent(parent, false);
            var rowElement = rowObject.GetComponent<LayoutElement>();
            rowElement.minHeight = 24f;
            rowElement.preferredHeight = 24f;

            var rowLayout = rowObject.GetComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 8f;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = true;

            var label = CreateText(rowObject.transform, 14, FontStyles.Normal);
            label.text = "Happiness";
            label.alignment = TextAlignmentOptions.MidlineLeft;
            label.GetComponent<LayoutElement>().minWidth = 88f;
            label.GetComponent<LayoutElement>().preferredWidth = 88f;

            var trackObject = new GameObject("HappinessTrack", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            trackObject.transform.SetParent(rowObject.transform, false);
            var trackElement = trackObject.GetComponent<LayoutElement>();
            trackElement.minHeight = 16f;
            trackElement.preferredHeight = 16f;
            trackElement.flexibleWidth = 1f;
            var trackImage = trackObject.GetComponent<Image>();
            trackImage.sprite = EnsureWhiteSprite();
            trackImage.type = Image.Type.Sliced;
            trackImage.color = new Color(0.18f, 0.2f, 0.28f, 1f);

            var fillObject = new GameObject("HappinessFill", typeof(RectTransform), typeof(Image));
            fillObject.transform.SetParent(trackObject.transform, false);
            var fillRect = fillObject.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(2f, 2f);
            fillRect.offsetMax = new Vector2(-2f, -2f);
            var fillImage = fillObject.GetComponent<Image>();
            fillImage.sprite = EnsureWhiteSprite();
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
            fillImage.color = new Color(0.95f, 0.55f, 0.72f, 1f);

            var valueText = CreateText(rowObject.transform, 16, FontStyles.Bold);
            valueText.alignment = TextAlignmentOptions.MidlineRight;
            valueText.GetComponent<LayoutElement>().minWidth = 36f;
            valueText.GetComponent<LayoutElement>().preferredWidth = 36f;

            return (fillImage, valueText);
        }

        TMP_Text CreateText(Transform parent, float fontSize, FontStyles style)
        {
            var textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            textObject.transform.SetParent(parent, false);
            textObject.GetComponent<LayoutElement>().minHeight = 18f;
            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.textWrappingMode = TextWrappingModes.Normal;
            return text;
        }

        static Sprite EnsureWhiteSprite()
        {
            if (whiteSprite != null)
                return whiteSprite;

            var texture = new Texture2D(4, 4);
            var pixels = new Color[16];
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;
            texture.SetPixels(pixels);
            texture.Apply();
            whiteSprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f), 4f);
            return whiteSprite;
        }
    }
}
