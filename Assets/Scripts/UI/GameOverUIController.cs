using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer.UI
{
    public class GameOverUIController : MonoBehaviour
    {
        public GameObject winPanelPrefab;
        public GameObject losePanelPrefab;
        public EndingCatalog endingCatalog;

        SessionModel session;
        GameOverPanelView activePanel;

        void Awake()
        {
            session = Simulation.GetModel<SessionModel>();
            RoundWon.OnExecute += OnWin;
            RoundLost.OnExecute += OnLose;
        }

        void OnDestroy()
        {
            RoundWon.OnExecute -= OnWin;
            RoundLost.OnExecute -= OnLose;
        }

        void OnWin(RoundWon _) => ShowEnding(true);
        void OnLose(RoundLost _) => ShowEnding(false);

        void ShowEnding(bool won)
        {
            if (activePanel != null)
                Destroy(activePanel.gameObject);

            var prefab = winPanelPrefab != null ? winPanelPrefab : losePanelPrefab;
            if (prefab == null)
                return;

            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
                return;

            var instance = Instantiate(prefab, canvas.transform);
            instance.name = won ? "DayEndingPanel" : "DayFailPanel";
            activePanel = instance.GetComponent<GameOverPanelView>();
            if (activePanel == null)
                return;

            var workers = FindObjectsByType<WorkerUnit>();
            var ending = endingCatalog != null
                ? endingCatalog.Resolve(session, workers, won)
                : null;

            var title = ending != null ? ending.title : won ? "Day Complete" : "Day Failed";
            var message = ending != null ? ending.description : won
                ? "The family made it through another day."
                : "The day ended before the family could recover.";
            var image = endingCatalog != null && ending != null
                ? endingCatalog.ResolveImage(ending)
                : null;

            var canAdvance = won && session.currentDay < SessionModel.TotalDays;
            var buttonLabel = canAdvance ? "Next Day" : "Play Again";
            var dayLabel = $"Day {session.currentDay} / {SessionModel.TotalDays}";

            activePanel.Configure(
                title,
                dayLabel,
                message,
                image,
                BuildStatsText(workers),
                buttonLabel,
                canAdvance ? AdvanceToNextDay : RestartSession);
        }

        string BuildStatsText(WorkerUnit[] workers)
        {
            if (session == null)
                return string.Empty;

            var summary = $"Hope {session.hope}  Stress {session.stress}  Rapport {session.rapport}";
            if (workers == null || workers.Length == 0)
                return summary;

            var lines = summary;
            for (var i = 0; i < workers.Length; i++)
            {
                if (workers[i] == null)
                    continue;

                lines += $"\n{workers[i].displayName}: {workers[i].GetAttributeSummary()}";
            }

            return lines;
        }

        public void AdvanceToNextDay()
        {
            if (activePanel != null)
            {
                Destroy(activePanel.gameObject);
                activePanel = null;
            }

            session.currentDay++;
            session.lastEndReason = RoundEndReason.None;

            var roundController = FindFirstObjectByType<RoundController>();
            if (roundController != null)
                roundController.StartRound();
        }

        public void RestartSession()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
