using System.Collections.Generic;
using Platformer.Mechanics;
using Platformer.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.UI
{
    public class WorkerRosterBarUI : MonoBehaviour
    {
        public GameObject charCardPrefab;
        public Camera worldCamera;

        RectTransform barRoot;
        Canvas rootCanvas;
        readonly List<CharCardPanelView> cards = new List<CharCardPanelView>();

        void Awake()
        {
            if (worldCamera == null)
                worldCamera = Camera.main;
            EnsureBar();
            RefreshCards();
        }

        void LateUpdate()
        {
            RefreshCards();
            SyncPickAreas();
        }

        public bool TryPickWorker(Vector2 screenPosition, out WorkerUnit worker)
        {
            worker = null;
            for (var i = cards.Count - 1; i >= 0; i--)
            {
                var card = cards[i];
                if (card == null || !card.gameObject.activeInHierarchy)
                    continue;
                if (!card.ContainsScreenPoint(screenPosition, GetEventCamera()))
                    continue;
                worker = card.BoundWorker;
                return worker != null && worker.state == WorkerState.InRoster;
            }

            return false;
        }

        void EnsureBar()
        {
            var hud = GameplayHUDView.Instance;
            if (hud != null)
            {
                hud.EnsureBindings();
                if (hud.rosterCardsRoot != null)
                    barRoot = hud.rosterCardsRoot;
                if (charCardPrefab == null)
                    charCardPrefab = hud.charCardPrefab;
            }

            if (barRoot != null)
            {
                rootCanvas = barRoot.GetComponentInParent<Canvas>();
                return;
            }

            var canvas = FindAnyObjectByType<Canvas>();
            if (canvas == null)
                return;

            rootCanvas = canvas;
            var barObject = new GameObject("WorkerRosterBar", typeof(RectTransform), typeof(Image), typeof(HorizontalLayoutGroup));
            barObject.transform.SetParent(canvas.transform, false);
            barRoot = barObject.GetComponent<RectTransform>();
            barRoot.anchorMin = new Vector2(0f, 0f);
            barRoot.anchorMax = new Vector2(1f, 0f);
            barRoot.pivot = new Vector2(0.5f, 0f);
            barRoot.sizeDelta = new Vector2(0f, 140f);
            barRoot.anchoredPosition = Vector2.zero;
            barObject.GetComponent<Image>().color = new Color(0.05f, 0.06f, 0.1f, 0.92f);

            var layout = barObject.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 10, 10);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
        }

        void RefreshCards()
        {
            if (barRoot == null)
                return;

            var workers = FindObjectsByType<WorkerUnit>();
            System.Array.Sort(workers, CompareWorkers);

            while (cards.Count < workers.Length)
                cards.Add(CreateCard());

            for (var i = 0; i < cards.Count; i++)
            {
                var visible = i < workers.Length;
                if (cards[i] != null)
                    cards[i].gameObject.SetActive(visible);
                if (!visible)
                    continue;

                var worker = workers[i];
                cards[i].BindWorker(worker);
                var highlighted = worker.state == WorkerState.Dragging || worker.state == WorkerState.Working;
                var portraitSprite = worker.GetPortraitSprite();
                cards[i].Bind(worker.displayName, portraitSprite, worker.attributes.happiness, highlighted);
            }
        }

        void SyncPickAreas()
        {
            if (worldCamera == null)
                worldCamera = Camera.main;
            if (worldCamera == null)
                return;

            var eventCamera = GetEventCamera();
            var depth = Mathf.Abs(worldCamera.transform.position.z);

            for (var i = 0; i < cards.Count; i++)
            {
                var card = cards[i];
                if (card == null || !card.gameObject.activeInHierarchy)
                    continue;

                var worker = card.BoundWorker;
                if (worker == null || worker.state != WorkerState.InRoster)
                    continue;

                var pickRect = card.PickRect;
                if (pickRect == null)
                    continue;

                var corners = new Vector3[4];
                pickRect.GetWorldCorners(corners);

                var blScreen = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[0]);
                var trScreen = RectTransformUtility.WorldToScreenPoint(eventCamera, corners[2]);
                var centerScreen = (blScreen + trScreen) * 0.5f;

                var centerWorld = worldCamera.ScreenToWorldPoint(new Vector3(centerScreen.x, centerScreen.y, depth));
                centerWorld.z = 0f;

                var worldBL = worldCamera.ScreenToWorldPoint(new Vector3(blScreen.x, blScreen.y, depth));
                var worldTR = worldCamera.ScreenToWorldPoint(new Vector3(trScreen.x, trScreen.y, depth));
                var worldSize = new Vector2(Mathf.Abs(worldTR.x - worldBL.x), Mathf.Abs(worldTR.y - worldBL.y));

                worker.SyncRosterPickArea(centerWorld, worldSize);
            }
        }

        static int CompareWorkers(WorkerUnit a, WorkerUnit b)
        {
            var orderA = GetRosterOrder(a);
            var orderB = GetRosterOrder(b);
            if (orderA != orderB)
                return orderA.CompareTo(orderB);
            return string.CompareOrdinal(a.displayName, b.displayName);
        }

        static int GetRosterOrder(WorkerUnit worker)
        {
            return worker.familyMember switch
            {
                FamilyMemberId.Dad => 0,
                FamilyMemberId.Mom => 1,
                FamilyMemberId.Sister => 2,
                FamilyMemberId.Brother => 3,
                _ => 100
            };
        }

        Camera GetEventCamera()
        {
            if (rootCanvas == null)
                rootCanvas = barRoot != null ? barRoot.GetComponentInParent<Canvas>() : null;
            if (rootCanvas == null || rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return null;
            return rootCanvas.worldCamera;
        }

        CharCardPanelView CreateCard()
        {
            GameObject cardObject;
            if (charCardPrefab != null)
            {
                cardObject = Instantiate(charCardPrefab, barRoot);
                cardObject.name = "CharCard";
            }
            else
            {
                cardObject = new GameObject("CharCard", typeof(RectTransform), typeof(Image));
                cardObject.transform.SetParent(barRoot, false);
            }

            var view = cardObject.GetComponent<CharCardPanelView>();
            if (view == null)
                view = cardObject.AddComponent<CharCardPanelView>();
            view.EnsureBindings();
            return view;
        }
    }
}
