using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.UI
{
    public class GameOverPanelView : MonoBehaviour
    {
        public TMP_Text titleText;
        public TMP_Text dayText;
        public TMP_Text messageText;
        public TMP_Text statsText;
        public Image endingImage;
        public Button restartButton;
        public TMP_Text restartButtonLabel;

        Action restartHandler;

        void OnDestroy()
        {
            if (restartButton != null && restartHandler != null)
                restartButton.onClick.RemoveListener(OnRestartClicked);
        }

        public void Configure(
            string title,
            string dayLabel,
            string message,
            Sprite image,
            string stats,
            string buttonLabel,
            Action onRestart)
        {
            if (titleText != null)
                titleText.text = title;

            if (dayText != null)
            {
                dayText.gameObject.SetActive(!string.IsNullOrEmpty(dayLabel));
                dayText.text = dayLabel;
            }

            if (messageText != null)
                messageText.text = message;

            if (endingImage != null)
            {
                endingImage.gameObject.SetActive(image != null);
                endingImage.sprite = image;
                endingImage.preserveAspect = true;
            }

            if (statsText != null)
                statsText.text = stats;

            if (restartButtonLabel != null)
                restartButtonLabel.text = buttonLabel;

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
