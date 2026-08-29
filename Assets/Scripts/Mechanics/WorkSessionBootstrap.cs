using Platformer.Core;
using Platformer.Model;
using Platformer.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.Mechanics
{
    public class WorkSessionBootstrap : MonoBehaviour
    {
        public string[] disableObjectNames =
        {
            "Player",
            "Grid",
            "Tokens",
            "Enemies",
            "Zones",
            "Background",
            "Alien",
            "CM vcam1",
            "SpawnPoint"
        };

        public float timeLimit = 90f;
        public int targetOutput = 50;
        public Canvas mainCanvas;
        public GameObject startPanel;

        static Sprite squareSprite;
        Transform workRoot;
        RoundController roundController;

        void Awake()
        {
            Simulation.SetModel(Simulation.GetModel<SessionModel>());
            DisableLegacyObjects();
            EnsureSquareSprite();
            EnsureCamera();
            BuildWorkFloor();
            EnsureUI();
            EnsureControllers();
        }

        void DisableLegacyObjects()
        {
            foreach (var objectName in disableObjectNames)
            {
                var target = GameObject.Find(objectName);
                if (target != null)
                    target.SetActive(false);
            }
        }

        void EnsureCamera()
        {
            if (FindFirstObjectByType<TopDownCameraSetup>() == null)
                gameObject.AddComponent<TopDownCameraSetup>();
        }

        void EnsureControllers()
        {
            if (GetComponent<RoundController>() == null)
                roundController = gameObject.AddComponent<RoundController>();
            else
                roundController = GetComponent<RoundController>();

            if (GetComponent<WorkerDragController>() == null)
                gameObject.AddComponent<WorkerDragController>();

            if (FindFirstObjectByType<RoundHUDController>() == null)
            {
                var hudObject = new GameObject("RoundHUD");
                hudObject.AddComponent<RoundHUDController>();
            }

            if (FindFirstObjectByType<SessionHUDController>() == null)
            {
                var sessionHudObject = new GameObject("SessionHUD");
                sessionHudObject.AddComponent<SessionHUDController>();
            }
        }

        void BuildWorkFloor()
        {
            workRoot = new GameObject("WorkSession").transform;
            workRoot.SetParent(transform, false);

            var stationsParent = new GameObject("Stations").transform;
            stationsParent.SetParent(workRoot, false);

            var rosterParent = new GameObject("Roster").transform;
            rosterParent.SetParent(workRoot, false);

            var stationA = CreateStation(stationsParent, "Builder Bay", WorkerRole.Builder, new Vector3(-2f, 2f, 0f), new Color(0.55f, 0.3f, 0.2f, 1f));
            var stationB = CreateStation(stationsParent, "Analysis Desk", WorkerRole.Analyst, new Vector3(2f, 2f, 0f), new Color(0.2f, 0.35f, 0.6f, 1f));
            var stationC = CreateStation(stationsParent, "Courier Hub", WorkerRole.Courier, new Vector3(-2f, -2f, 0f), new Color(0.2f, 0.5f, 0.25f, 1f));
            var stationD = CreateStation(stationsParent, "Open Floor", WorkerRole.Any, new Vector3(2f, -2f, 0f), new Color(0.45f, 0.4f, 0.55f, 1f));

            roundController = GetComponent<RoundController>();
            if (roundController == null)
                roundController = gameObject.AddComponent<RoundController>();
            roundController.stations = new[] { stationA, stationB, stationC, stationD };

            var rosterPositions = new[]
            {
                new Vector3(-6f, 1.5f, 0f),
                new Vector3(-6f, 0.5f, 0f),
                new Vector3(-6f, -0.5f, 0f),
                new Vector3(-6f, -1.5f, 0f)
            };

            var roles = new[]
            {
                WorkerRole.Builder,
                WorkerRole.Analyst,
                WorkerRole.Courier,
                WorkerRole.Builder
            };

            for (var i = 0; i < rosterPositions.Length; i++)
                CreateWorker(rosterParent, roles[i], rosterPositions[i]);

            CreateLabel(rosterParent, "Workers", new Vector3(-6f, 2.5f, 0f));
            CreateLabel(stationsParent, "Work Stations", new Vector3(0f, 3.5f, 0f));
        }

        WorkStation CreateStation(Transform parent, string label, WorkerRole role, Vector3 position, Color color)
        {
            var stationObject = new GameObject(label, typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(WorkStation));
            stationObject.transform.SetParent(parent, false);
            stationObject.transform.position = position;
            stationObject.transform.localScale = new Vector3(2.4f, 1.6f, 1f);

            var sprite = stationObject.GetComponent<SpriteRenderer>();
            sprite.sprite = squareSprite;
            sprite.color = color;
            sprite.sortingOrder = 1;

            var collider = stationObject.GetComponent<BoxCollider2D>();
            collider.isTrigger = false;

            var station = stationObject.GetComponent<WorkStation>();
            station.stationId = label;
            station.requiredRole = role;
            station.capacity = 2;
            station.outputPerWorker = 3f;

            CreateLabel(stationObject.transform, label, new Vector3(0f, 1.1f, 0f));
            return station;
        }

        void CreateWorker(Transform parent, WorkerRole role, Vector3 homePosition)
        {
            var workerObject = new GameObject($"Worker_{role}", typeof(SpriteRenderer), typeof(CircleCollider2D), typeof(WorkerUnit));
            workerObject.transform.SetParent(parent, false);
            workerObject.transform.localScale = Vector3.one * 0.7f;

            var sprite = workerObject.GetComponent<SpriteRenderer>();
            sprite.sprite = squareSprite;
            sprite.sortingOrder = 10;

            var collider = workerObject.GetComponent<CircleCollider2D>();
            collider.radius = 0.45f;

            var worker = workerObject.GetComponent<WorkerUnit>();
            worker.Initialize(role, homePosition);
        }

        void CreateLabel(Transform parent, string text, Vector3 localPosition)
        {
            var labelObject = new GameObject("Label", typeof(TextMeshPro));
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = localPosition;
            var label = labelObject.GetComponent<TextMeshPro>();
            label.text = text;
            label.fontSize = 3f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
        }

        void EnsureUI()
        {
            if (mainCanvas == null)
                mainCanvas = FindFirstObjectByType<Canvas>();
            if (mainCanvas != null)
                mainCanvas.gameObject.SetActive(true);
            EnsureStartPanel();
        }

        void EnsureStartPanel()
        {
            if (startPanel != null)
                return;

            var canvas = mainCanvas != null ? mainCanvas : FindFirstObjectByType<Canvas>();
            if (canvas == null)
                return;

            startPanel = new GameObject("StartPanel", typeof(RectTransform), typeof(Image));
            startPanel.transform.SetParent(canvas.transform, false);
            var panelImage = startPanel.GetComponent<Image>();
            panelImage.color = new Color(0.05f, 0.07f, 0.12f, 0.94f);

            var panelRect = startPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;

            var title = CreateUiText(startPanel.transform, "Worker Shift", 48, new Vector2(0.1f, 0.55f), new Vector2(0.9f, 0.8f));
            var subtitle = CreateUiText(startPanel.transform, "Drag workers to matching stations before time runs out.", 24, new Vector2(0.12f, 0.42f), new Vector2(0.88f, 0.54f));
            subtitle.color = new Color(0.8f, 0.85f, 0.95f, 1f);

            var buttonObject = new GameObject("StartButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(startPanel.transform, false);
            var buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.35f, 0.22f);
            buttonRect.anchorMax = new Vector2(0.65f, 0.34f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;
            buttonObject.GetComponent<Image>().color = new Color(0.22f, 0.45f, 0.78f, 1f);

            var label = CreateUiText(buttonObject.transform, "Start Shift", 28, Vector2.zero, Vector2.one);
            buttonObject.GetComponent<Button>().onClick.AddListener(BeginSession);
        }

        TMP_Text CreateUiText(Transform parent, string text, float fontSize, Vector2 anchorMin, Vector2 anchorMax)
        {
            var textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var label = textObject.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            return label;
        }

        public void BeginSession()
        {
            var session = Simulation.GetModel<SessionModel>();
            session.round.timeLimit = timeLimit;
            session.round.targetOutput = targetOutput;

            if (startPanel != null)
                startPanel.SetActive(false);

            if (roundController == null)
                roundController = GetComponent<RoundController>();
            roundController.StartRound();

            var eventController = GetComponent<RandomEventController>();
            if (eventController != null)
                eventController.StartSession();
        }

        static void EnsureSquareSprite()
        {
            if (squareSprite != null)
                return;

            var texture = new Texture2D(4, 4);
            var pixels = new Color[16];
            for (var i = 0; i < pixels.Length; i++)
                pixels[i] = Color.white;
            texture.SetPixels(pixels);
            texture.Apply();
            squareSprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f), 4f);
        }
    }
}
