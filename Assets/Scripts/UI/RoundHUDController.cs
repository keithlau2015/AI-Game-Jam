using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Model;
using TMPro;
using UnityEngine;

namespace Platformer.UI
{
    public class RoundHUDController : MonoBehaviour
    {
        public TMP_Text timerText;
        public TMP_Text statusText;

        SessionModel session;
        TMP_Text dayText;
        TMP_Text timeText;

        void Awake()
        {
            session = Simulation.GetModel<SessionModel>();
            BindHudTexts();
            EnsureHUD();
            RoundWon.OnExecute += OnRoundEnded;
            RoundLost.OnExecute += OnRoundEnded;
            RandomEventTriggered.OnExecute += OnEventChanged;
            RandomEventResolved.OnExecute += OnEventChanged;
        }

        void OnDestroy()
        {
            RoundWon.OnExecute -= OnRoundEnded;
            RoundLost.OnExecute -= OnRoundEnded;
            RandomEventTriggered.OnExecute -= OnEventChanged;
            RandomEventResolved.OnExecute -= OnEventChanged;
        }

        void Update()
        {
            Refresh();
        }

        void OnRoundEnded(RoundWon _) => Refresh();
        void OnRoundEnded(RoundLost _) => Refresh();
        void OnEventChanged(RandomEventTriggered _) => Refresh();
        void OnEventChanged(RandomEventResolved _) => Refresh();

        void BindHudTexts()
        {
            var hud = GameplayHUDView.Instance;
            if (hud == null)
                return;

            hud.EnsureBindings();
            dayText = hud.dayText;
            timeText = hud.timeText;
            if (statusText == null)
                statusText = hud.statusText;
        }

        void Refresh()
        {
            if (session == null)
                return;

            var seconds = Mathf.CeilToInt(session.round.timeRemaining);
            var minutes = seconds / 60;
            var remainder = seconds % 60;

            if (dayText != null)
                dayText.text = $"{session.currentDay}<size=50%>/{SessionModel.TotalDays}</size>";

            if (timeText != null)
                timeText.text = $"{minutes:00}:{remainder:00}";

            if (timerText != null)
                timerText.text = $"Day {session.currentDay}/{SessionModel.TotalDays}  {minutes:00}:{remainder:00}";

            if (statusText != null)
            {
                statusText.text = session.round.phase switch
                {
                    RoundPhase.Won => "Shift complete",
                    RoundPhase.Lost => "Shift failed",
                    RoundPhase.PausedForEvent => "Decision required",
                    RoundPhase.Playing => "Assign workers",
                    _ => "Ready"
                };
            }
        }

        void EnsureHUD()
        {
            if (dayText != null || timerText != null)
                return;

            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
                return;

            var hudRoot = new GameObject("RoundHUD", typeof(RectTransform));
            hudRoot.transform.SetParent(canvas.transform, false);
            var rect = hudRoot.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 56f);
            rect.anchoredPosition = new Vector2(0f, -72f);

            timerText = CreateLabel(hudRoot.transform, new Vector2(0.05f, 0.5f), TextAlignmentOptions.Left);
            statusText = CreateLabel(hudRoot.transform, new Vector2(0.82f, 0.5f), TextAlignmentOptions.Left);
        }

        TMP_Text CreateLabel(Transform parent, Vector2 anchor, TextAlignmentOptions alignment)
        {
            var textObject = new GameObject("Stat", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(alignment == TextAlignmentOptions.Left ? 0f : 0.5f, 0.5f);
            rect.sizeDelta = new Vector2(260f, 40f);
            var text = textObject.GetComponent<TextMeshProUGUI>();
            UIFontProvider.Apply(text);
            text.fontSize = 24;
            text.alignment = alignment;
            text.color = Color.white;
            return text;
        }
    }
}
