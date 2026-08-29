using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Model;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Platformer.UI
{
    public class GameOverUIController : MonoBehaviour
    {
        public GameObject winPanelPrefab;
        public GameObject losePanelPrefab;

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

        void OnWin(RoundWon _) => ShowPanel(true);
        void OnLose(RoundLost _) => ShowPanel(false);

        void ShowPanel(bool won)
        {
            if (activePanel != null)
                Destroy(activePanel.gameObject);

            var prefab = won ? winPanelPrefab : losePanelPrefab;
            if (prefab == null)
                return;

            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
                canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
                return;

            var instance = Instantiate(prefab, canvas.transform);
            instance.name = won ? "WinPanel" : "LosePanel";
            activePanel = instance.GetComponent<GameOverPanelView>();
            if (activePanel == null)
                return;

            var stats = BuildStatsText();
            if (won)
            {
                activePanel.Configure(
                    "Victory",
                    "Your team reached a winning milestone.",
                    stats,
                    RestartSession);
            }
            else
            {
                activePanel.Configure(
                    "Defeat",
                    "A critical value dropped too low.",
                    stats,
                    RestartSession);
            }
        }

        string BuildStatsText()
        {
            if (session == null)
                return string.Empty;

            return $"Karma {session.karma}   Morale {session.morale}   Reputation {session.reputation}\nOutput {session.round.currentOutput}/{session.round.targetOutput}";
        }

        public void RestartSession()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
