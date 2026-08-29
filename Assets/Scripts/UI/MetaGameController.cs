using Platformer.Core;
using Platformer.Model;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Platformer.UI
{
    public class MetaGameController : MonoBehaviour
    {
        public MainUIController mainMenu;
        public Canvas[] gamePlayCanvasii;
        public int pausePanelIndex = 1;
        public string pausePanelName = "Settings";

        SessionModel session;
        InputAction menuAction;
        bool pauseOpen;

        void Awake()
        {
            session = Simulation.GetModel<SessionModel>();
        }

        void OnEnable()
        {
            menuAction = InputSystem.actions.FindAction("Player/Menu");
            menuAction?.Enable();
            ClosePauseMenu(resumeTime: true);
        }

        void OnDisable()
        {
            menuAction?.Disable();
        }

        void Update()
        {
            if (menuAction == null || !menuAction.WasPressedThisFrame())
                return;

            if (pauseOpen)
                ClosePauseMenu(resumeTime: CanResumeGameplay());
            else
                OpenPauseMenu();
        }

        public void TogglePauseMenu(bool show)
        {
            if (show)
                OpenPauseMenu();
            else
                ClosePauseMenu(resumeTime: CanResumeGameplay());
        }

        void OpenPauseMenu()
        {
            if (mainMenu == null || pauseOpen || !CanOpenPauseMenu())
                return;

            pauseOpen = true;
            mainMenu.gameObject.SetActive(true);
            ShowPausePanel();
            Time.timeScale = 0f;
            if (session != null)
                session.round.dragEnabled = false;
        }

        void ClosePauseMenu(bool resumeTime)
        {
            if (mainMenu == null)
                return;

            pauseOpen = false;
            mainMenu.HideAllPanels();

            if (resumeTime && CanResumeGameplay())
                Time.timeScale = 1f;

            if (session != null && session.round.phase == RoundPhase.Playing && !session.eventState.awaitingDecision)
                session.round.dragEnabled = true;
        }

        void ShowPausePanel()
        {
            var panel = mainMenu.FindPanel(pausePanelName);
            if (panel != null)
            {
                mainMenu.HideAllPanels();
                panel.SetActive(true);
                return;
            }

            if (pausePanelIndex >= 0 && mainMenu.panels != null && pausePanelIndex < mainMenu.panels.Length)
                mainMenu.SetActivePanel(pausePanelIndex);
        }

        bool CanOpenPauseMenu()
        {
            if (session == null)
                return true;

            if (session.round.phase == RoundPhase.Won || session.round.phase == RoundPhase.Lost)
                return false;

            if (session.eventState.awaitingDecision)
                return false;

            return true;
        }

        bool CanResumeGameplay()
        {
            if (session == null)
                return true;

            return !session.eventState.awaitingDecision
                && session.round.phase != RoundPhase.Won
                && session.round.phase != RoundPhase.Lost;
        }
    }
}
