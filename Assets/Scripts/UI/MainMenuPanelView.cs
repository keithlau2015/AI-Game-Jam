using Platformer.Core;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Platformer.UI
{
    public class MainMenuPanelView : MonoBehaviour
    {
        public string gameSceneName = GameScenes.Game;
        public TMP_Text titleText;
        public TMP_Text subtitleText;
        public Button startButton;

        void Awake()
        {
            if (startButton != null)
                startButton.onClick.AddListener(StartGame);
        }

        void OnDestroy()
        {
            if (startButton != null)
                startButton.onClick.RemoveListener(StartGame);
        }

        public void StartGame()
        {
            SceneManager.LoadScene(gameSceneName);
        }
    }
}
