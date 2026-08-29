using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.UI
{
    public class GameOverPanelView : MonoBehaviour
    {
        public TMP_Text titleText;
        public TMP_Text messageText;
        public TMP_Text statsText;
        public Button restartButton;

        Action restartHandler;

        void OnDestroy()
        {
            if (restartButton != null && restartHandler != null)
                restartButton.onClick.RemoveListener(OnRestartClicked);
        }

        public void Configure(string title, string message, string stats, Action onRestart)
        {
            if (titleText != null)
                titleText.text = title;
            if (messageText != null)
                messageText.text = message;
            if (statsText != null)
                statsText.text = stats;

            if (restartButton != null)
            {
                if (restartHandler != null)
                    restartButton.onClick.RemoveListener(OnRestartClicked);
                restartHandler = onRestart;
                if (restartHandler != null)
                    restartButton.onClick.AddListener(OnRestartClicked);
            }
        }

        void OnRestartClicked()
        {
            restartHandler?.Invoke();
        }
    }
}
