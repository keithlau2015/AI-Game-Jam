using TMPro;
using UnityEngine;

namespace Platformer.UI
{
    public class GameplayHUDView : MonoBehaviour
    {
        public static GameplayHUDView Instance { get; private set; }

        public RectTransform familyStatListRoot;
        public FamilyStatBarPanelView hopeBar;
        public FamilyStatBarPanelView stressBar;
        public FamilyStatBarPanelView rapportBar;

        public RectTransform rosterCardsRoot;
        public GameObject charCardPrefab;

        public RectTransform datePanelRoot;
        public TMP_Text dayText;
        public TMP_Text timeText;
        public TMP_Text statusText;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            EnsureBindings();
            UIFontProvider.ApplyToHierarchy(transform);
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void EnsureBindings()
        {
            if (familyStatListRoot == null)
            {
                var list = transform.Find("FamilyStatic_list");
                if (list != null)
                    familyStatListRoot = list.GetComponent<RectTransform>();
            }

            if (rosterCardsRoot == null)
            {
                var roster = transform.Find("ChatCarts");
                if (roster != null)
                    rosterCardsRoot = roster.GetComponent<RectTransform>();
            }

            if (hopeBar == null || stressBar == null || rapportBar == null)
                BindFamilyStatBars();

            if (dayText == null || timeText == null)
                BindDateTexts();

            ApplyDatePanelLayout();
            HideBackgroundLayer();
        }

        void BindFamilyStatBars()
        {
            if (familyStatListRoot == null)
                return;

            var bars = familyStatListRoot.GetComponentsInChildren<FamilyStatBarPanelView>(true);
            if (bars.Length == 0)
            {
                for (var i = 0; i < familyStatListRoot.childCount; i++)
                {
                    var child = familyStatListRoot.GetChild(i);
                    if (child.GetComponent<FamilyStatBarPanelView>() == null)
                        child.gameObject.AddComponent<FamilyStatBarPanelView>();
                }

                bars = familyStatListRoot.GetComponentsInChildren<FamilyStatBarPanelView>(true);
            }

            if (bars.Length > 0)
            {
                hopeBar = bars[0];
                hopeBar.SetLabel("Hope");
            }

            if (bars.Length > 1)
            {
                stressBar = bars[1];
                stressBar.SetLabel("Stress");
            }

            if (bars.Length > 2)
            {
                rapportBar = bars[2];
                rapportBar.SetLabel("Rapport");
            }
        }

        void BindDateTexts()
        {
            if (datePanelRoot == null)
            {
                var date = transform.Find("date");
                if (date != null)
                    datePanelRoot = date.GetComponent<RectTransform>();
            }

            if (datePanelRoot == null)
                return;

            var texts = datePanelRoot.GetComponentsInChildren<TMP_Text>(true);
            if (texts.Length > 0)
                dayText = texts[0];
            if (texts.Length > 1)
                timeText = texts[1];
        }

        void ApplyDatePanelLayout()
        {
            if (datePanelRoot == null)
            {
                var date = transform.Find("date");
                if (date != null)
                    datePanelRoot = date.GetComponent<RectTransform>();
            }

            if (datePanelRoot == null)
                return;

            datePanelRoot.SetAsLastSibling();
            datePanelRoot.anchorMin = new Vector2(1f, 1f);
            datePanelRoot.anchorMax = new Vector2(1f, 1f);
            datePanelRoot.pivot = new Vector2(1f, 1f);
            datePanelRoot.anchoredPosition = new Vector2(-24f, -24f);
            datePanelRoot.sizeDelta = new Vector2(300f, 260f);
        }

        void HideBackgroundLayer()
        {
            var bg = transform.Find("Bg");
            if (bg != null)
                bg.gameObject.SetActive(false);
        }
    }
}
