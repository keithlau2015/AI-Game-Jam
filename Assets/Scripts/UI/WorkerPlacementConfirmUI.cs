using System;
using Platformer.Mechanics;
using Platformer.Model;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.UI
{
    public class WorkerPlacementConfirmUI : MonoBehaviour
    {
        public Canvas targetCanvas;

        GameObject panelRoot;
        TMP_Text messageText;
        Button confirmButton;
        Button cancelButton;
        Action onConfirm;
        Action onCancel;

        public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

        void Awake()
        {
            EnsureUI();
            Hide();
        }

        public void Show(WorkerUnit worker, WorkStation station, Action confirm, Action cancel)
        {
            if (worker == null || station == null)
                return;

            EnsureUI();
            onConfirm = confirm;
            onCancel = cancel;
            messageText.text = BuildMessage(worker, station);
            panelRoot.SetActive(true);
        }

        string BuildMessage(WorkerUnit worker, WorkStation station)
        {
            var roleText = station.requiredRole == WorkerRole.Any ? "Any role" : station.requiredRole.ToString();
            var jobRole = worker.GetJobRoleForStation(station);
            var skill = worker.GetSkillForStation(station);
            return $"Assign {worker.displayName} to {station.stationId}?\nRequired: {roleText}   Capacity: {station.AssignedWorkers.Count + 1}/{station.capacity}\n{jobRole} trait {skill}   {worker.GetAttributeSummary()}";
        }

        public void Hide()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
            onConfirm = null;
            onCancel = null;
        }

        void EnsureUI()
        {
            if (panelRoot != null)
                return;

            var canvas = targetCanvas;
            if (canvas == null)
            {
                var canvasObject = GameObject.Find("UI Canvas");
                if (canvasObject != null)
                    canvas = canvasObject.GetComponent<Canvas>();
            }

            if (canvas == null)
                canvas = FindAnyObjectByType<Canvas>();

            if (canvas == null)
                return;

            panelRoot = new GameObject("WorkerPlacementConfirm", typeof(RectTransform), typeof(Image));
            panelRoot.transform.SetParent(canvas.transform, false);
            var panelImage = panelRoot.GetComponent<Image>();
            panelImage.color = new Color(0f, 0f, 0f, 0.55f);

            var panelRect = panelRoot.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var card = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
            card.transform.SetParent(panelRoot.transform, false);
            var cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.28f, 0.38f);
            cardRect.anchorMax = new Vector2(0.72f, 0.62f);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;
            card.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.18f, 0.98f);
            var layout = card.GetComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(24, 24, 24, 24);
            layout.spacing = 20f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            messageText = CreateText(card.transform, 26, FontStyles.Normal);
            var buttonRow = new GameObject("Buttons", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            buttonRow.transform.SetParent(card.transform, false);
            var rowLayout = buttonRow.GetComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 16f;
            rowLayout.childAlignment = TextAnchor.MiddleCenter;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            rowLayout.childForceExpandWidth = true;
            rowLayout.childForceExpandHeight = true;
            var rowElement = buttonRow.AddComponent<LayoutElement>();
            rowElement.minHeight = 48f;

            confirmButton = CreateButton(buttonRow.transform, "Confirm", new Color(0.22f, 0.55f, 0.35f, 1f));
            cancelButton = CreateButton(buttonRow.transform, "Cancel", new Color(0.55f, 0.25f, 0.25f, 1f));
            confirmButton.onClick.AddListener(HandleConfirm);
            cancelButton.onClick.AddListener(HandleCancel);
        }

        void HandleConfirm()
        {
            var callback = onConfirm;
            Hide();
            callback?.Invoke();
        }

        void HandleCancel()
        {
            var callback = onCancel;
            Hide();
            callback?.Invoke();
        }

        TMP_Text CreateText(Transform parent, float fontSize, FontStyles style)
        {
            var textObject = new GameObject("Message", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.textWrappingMode = TextWrappingModes.Normal;
            var element = textObject.AddComponent<LayoutElement>();
            element.minHeight = 72f;
            element.flexibleHeight = 1f;
            return text;
        }

        Button CreateButton(Transform parent, string label, Color color)
        {
            var buttonObject = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            buttonObject.GetComponent<Image>().color = color;

            var textObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(buttonObject.transform, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = 22;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;

            return buttonObject.GetComponent<Button>();
        }
    }
}
