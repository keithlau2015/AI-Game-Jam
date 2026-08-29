using System.Collections.Generic;
using UnityEngine;

namespace Platformer.UI
{
    public class MainUIController : MonoBehaviour
    {
        public GameObject[] panels;
        public bool openFirstPanelOnEnable;

        public void SetActivePanel(int index)
        {
            if (panels == null)
                return;

            for (var i = 0; i < panels.Length; i++)
            {
                var active = i == index;
                var panel = panels[i];
                if (panel != null && panel.activeSelf != active)
                    panel.SetActive(active);
            }
        }

        public void HideAllPanels()
        {
            if (panels == null)
                return;

            for (var i = 0; i < panels.Length; i++)
            {
                if (panels[i] != null)
                    panels[i].SetActive(false);
            }
        }

        public GameObject FindPanel(string panelName)
        {
            if (panels == null || string.IsNullOrEmpty(panelName))
                return null;

            for (var i = 0; i < panels.Length; i++)
            {
                var panel = panels[i];
                if (panel != null && panel.name == panelName)
                    return panel;
            }

            return null;
        }

        void OnEnable()
        {
            if (openFirstPanelOnEnable && panels != null && panels.Length > 0)
                SetActivePanel(0);
        }
    }
}
