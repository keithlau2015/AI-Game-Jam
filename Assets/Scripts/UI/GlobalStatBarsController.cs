using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.UI
{
    public class GlobalStatBarsController : MonoBehaviour
    {
        static readonly Color HopeColor = new Color(0.95f, 0.78f, 0.28f, 1f);
        static readonly Color StressColor = new Color(0.92f, 0.38f, 0.38f, 1f);
        static readonly Color RapportColor = new Color(0.38f, 0.72f, 0.95f, 1f);
        static readonly Color TrackColor = new Color(0.12f, 0.14f, 0.18f, 0.95f);

        public GlobalStatBarView hopeBar;
        public GlobalStatBarView stressBar;
        public GlobalStatBarView rapportBar;

        SessionModel session;

        void Awake()
        {
            session = Simulation.GetModel<SessionModel>();
            EnsureUI();
            RandomEventTriggered.OnExecute += OnChanged;
            RandomEventResolved.OnExecute += OnChanged;
            RoundWon.OnExecute += OnRoundEnded;
            RoundLost.OnExecute += OnRoundEnded;
            Refresh();
        }

        void OnDestroy()
        {
            RandomEventTriggered.OnExecute -= OnChanged;
            RandomEventResolved.OnExecute -= OnChanged;
            RoundWon.OnExecute -= OnRoundEnded;
            RoundLost.OnExecute -= OnRoundEnded;
        }

        void Update()
        {
            Refresh();
        }

        void OnChanged(RandomEventTriggered _) => Refresh();
        void OnChanged(RandomEventResolved _) => Refresh();
        void OnRoundEnded(RoundWon _) => Refresh();
        void OnRoundEnded(RoundLost _) => Refresh();

        public void Refresh()
        {
            if (session == null || hopeBar == null)
                return;

            hopeBar.Set("Hope", session.hope, session.statMin, session.statMax, HopeColor);
            stressBar.Set("Stress", session.stress, session.statMin, session.statMax, StressColor);
            rapportBar.Set("Rapport", session.rapport, session.statMin, session.statMax, RapportColor);
        }

        void EnsureUI()
        {
            if (hopeBar != null)
                return;

            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
                return;

            var root = new GameObject("GlobalStatBars", typeof(RectTransform), typeof(VerticalLayoutGroup));
            root.transform.SetParent(canvas.transform, false);
            var rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0f, 1f);
            rootRect.anchorMax = new Vector2(0f, 1f);
            rootRect.pivot = new Vector2(0f, 1f);
            rootRect.anchoredPosition = new Vector2(16f, -132f);
            rootRect.sizeDelta = new Vector2(320f, 104f);

            var layout = root.GetComponent<VerticalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.UpperLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            hopeBar = CreateBarRow(root.transform, "Hope");
            stressBar = CreateBarRow(root.transform, "Stress");
            rapportBar = CreateBarRow(root.transform, "Rapport");
        }

        GlobalStatBarView CreateBarRow(Transform parent, string label)
        {
            var row = new GameObject(label + "Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            row.transform.SetParent(parent, false);
            var rowElement = row.GetComponent<LayoutElement>();
            rowElement.minHeight = 28f;
            rowElement.preferredHeight = 28f;

            var rowLayout = row.GetComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 8f;
            rowLayout.childAlignment = TextAnchor.MiddleLeft;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = true;

            var labelText = CreateLabel(row.transform, label, 72f, TextAlignmentOptions.MidlineLeft);
            var barRoot = CreateBarTrack(row.transform);
            var valueText = CreateLabel(row.transform, "0", 40f, TextAlignmentOptions.MidlineRight);

            return new GlobalStatBarView
            {
                labelText = labelText,
                fillImage = barRoot.fillImage,
                fillRect = barRoot.fillRect,
                valueText = valueText
            };
        }

        (Image fillImage, RectTransform fillRect) CreateBarTrack(Transform parent)
        {
            var track = new GameObject("Track", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            track.transform.SetParent(parent, false);
            var trackElement = track.GetComponent<LayoutElement>();
            trackElement.flexibleWidth = 1f;
            trackElement.minWidth = 180f;
            trackElement.preferredHeight = 18f;
            track.GetComponent<Image>().color = TrackColor;

            var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(track.transform, false);
            var fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            var fillImage = fill.GetComponent<Image>();
            fillImage.color = Color.white;

            return (fillImage, fillRect);
        }

        TMP_Text CreateLabel(Transform parent, string text, float width, TextAlignmentOptions alignment)
        {
            var textObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            textObject.transform.SetParent(parent, false);
            var element = textObject.GetComponent<LayoutElement>();
            element.minWidth = width;
            element.preferredWidth = width;
            var label = textObject.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = 18;
            label.alignment = alignment;
            label.color = Color.white;
            return label;
        }
    }
}
