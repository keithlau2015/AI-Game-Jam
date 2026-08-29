using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Model;
using TMPro;
using UnityEngine;

namespace Platformer.UI
{
    public class SessionHUDController : MonoBehaviour
    {
        public TMP_Text statusText;

        SessionModel model;

        void Awake()
        {
            model = Simulation.GetModel<SessionModel>();
            EnsureHUD();
            RandomEventTriggered.OnExecute += OnEventChanged;
            RandomEventResolved.OnExecute += OnEventChanged;
            RoundWon.OnExecute += OnRoundEnded;
            RoundLost.OnExecute += OnRoundEnded;
            Refresh();
        }

        void OnDestroy()
        {
            RandomEventTriggered.OnExecute -= OnEventChanged;
            RandomEventResolved.OnExecute -= OnEventChanged;
            RoundWon.OnExecute -= OnRoundEnded;
            RoundLost.OnExecute -= OnRoundEnded;
        }

        void OnEventChanged(RandomEventTriggered _) => Refresh();
        void OnEventChanged(RandomEventResolved _) => Refresh();
        void OnRoundEnded(RoundWon _) => Refresh();
        void OnRoundEnded(RoundLost _) => Refresh();

        void Update()
        {
            Refresh();
        }

        public void Refresh()
        {
            if (model == null)
                return;

            if (statusText != null)
            {
                if (model.eventState.awaitingDecision)
                    statusText.text = "Decision required";
                else if (model.round.phase == RoundPhase.Won)
                    statusText.text = "Victory";
                else if (model.round.phase == RoundPhase.Lost)
                    statusText.text = "Failed";
                else if (!model.sessionStarted)
                    statusText.text = "Awaiting start";
                else
                    statusText.text = $"Events resolved: {model.eventsResolved}";
            }
        }

        void EnsureHUD()
        {
            if (statusText != null)
                return;

            var hud = GameplayHUDView.Instance;
            if (hud != null)
            {
                hud.EnsureBindings();
                statusText = hud.statusText;
            }

            if (statusText != null)
                return;

            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                canvas = FindFirstObjectByType<Canvas>();

            if (canvas == null)
                return;

            var hudRoot = new GameObject("SessionHUD", typeof(RectTransform));
            hudRoot.transform.SetParent(canvas.transform, false);
            var rect = hudRoot.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 48f);
            rect.anchoredPosition = new Vector2(0f, -8f);

            statusText = CreateStatLabel(hudRoot.transform, new Vector2(0.82f, 0.5f), TextAlignmentOptions.Left);
        }

        TMP_Text CreateStatLabel(Transform parent, Vector2 anchor, TextAlignmentOptions alignment)
        {
            var textObject = new GameObject("Stat", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(alignment == TextAlignmentOptions.Left ? 0f : 0.5f, 0.5f);
            rect.sizeDelta = new Vector2(280f, 36f);
            var text = textObject.GetComponent<TextMeshProUGUI>();
            UIFontProvider.Apply(text);
            text.fontSize = 22;
            text.alignment = alignment;
            text.color = Color.white;
            return text;
        }
    }
}
