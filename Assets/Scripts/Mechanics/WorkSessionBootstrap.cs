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

        public WorkFloorLayout floorLayout;
        public float timeLimit = 120f;
        public bool autoStartOnLoad = true;
        public Canvas mainCanvas;
        public GameObject startPanel;
        public GameObject winPanelPrefab;
        public GameObject losePanelPrefab;
        public EndingCatalog endingCatalog;
        public GameObject gameplayHudPrefab;
        public PopupTaskPool popupTaskPool;

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

        void Start()
        {
            if (autoStartOnLoad)
                BeginSession();
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

            if (FindFirstObjectByType<GlobalStatBarsController>() == null)
            {
                var statBarsObject = new GameObject("GlobalStatBars");
                statBarsObject.AddComponent<GlobalStatBarsController>();
            }

            if (FindFirstObjectByType<WorkerPlacementConfirmUI>() == null)
            {
                var confirmObject = new GameObject("WorkerPlacementConfirmUI");
                confirmObject.AddComponent<WorkerPlacementConfirmUI>();
            }

            if (FindFirstObjectByType<WorkerRosterBarUI>() == null)
            {
                var rosterBarObject = new GameObject("WorkerRosterBarUI");
                rosterBarObject.AddComponent<WorkerRosterBarUI>();
            }

            EnsurePopupTaskSpawner();
            EnsureGameOverUI();
            EnsureGameAudio();
        }

        void EnsureGameAudio()
        {
            if (GetComponent<GameAudioController>() == null)
                gameObject.AddComponent<GameAudioController>();
        }

        void EnsurePopupTaskSpawner()
        {
            var spawner = GetComponent<PopupTaskSpawner>();
            if (spawner == null)
                spawner = gameObject.AddComponent<PopupTaskSpawner>();
            spawner.floorLayout = floorLayout;
            if (popupTaskPool == null)
                popupTaskPool = Resources.Load<PopupTaskPool>("FamilyPopupTaskPool");
            spawner.taskPool = popupTaskPool;
            spawner.maxActiveTasks = popupTaskPool != null ? popupTaskPool.maxConcurrentTasks : 3;
        }

        void EnsureGameOverUI()
        {
            var canvas = mainCanvas != null ? mainCanvas : FindFirstObjectByType<Canvas>();
            if (canvas == null)
                return;

            var controller = FindFirstObjectByType<GameOverUIController>();
            if (controller == null)
            {
                var controllerObject = new GameObject("GameOverUI", typeof(RectTransform), typeof(GameOverUIController));
                controllerObject.transform.SetParent(canvas.transform, false);
                var rect = controllerObject.GetComponent<RectTransform>();
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = Vector2.zero;
                controller = controllerObject.GetComponent<GameOverUIController>();
            }

            if (winPanelPrefab != null)
                controller.winPanelPrefab = winPanelPrefab;
            if (losePanelPrefab != null)
                controller.losePanelPrefab = losePanelPrefab;
            if (endingCatalog != null)
                controller.endingCatalog = endingCatalog;
        }

        void BuildWorkFloor()
        {
            workRoot = new GameObject("WorkSession").transform;
            workRoot.SetParent(transform, false);

            if (floorLayout != null)
            {
                ApplyCameraFromLayout();
                CreateBackgroundFromLayout();
            }

            var stationsParent = new GameObject("Stations").transform;
            stationsParent.SetParent(workRoot, false);

            var rosterParent = new GameObject("Roster").transform;
            rosterParent.SetParent(workRoot, false);

            WorkStation[] stations;
            if (floorLayout != null && floorLayout.stations != null && floorLayout.stations.Length > 0)
                stations = BuildStationsFromLayout(stationsParent);
            else
                stations = BuildDefaultStations(stationsParent);

            roundController = GetComponent<RoundController>();
            if (roundController == null)
                roundController = gameObject.AddComponent<RoundController>();
            roundController.stations = stations;

            if (floorLayout != null && floorLayout.rosterSlots != null && floorLayout.rosterSlots.Length > 0)
                BuildRosterFromLayout(rosterParent);
            else
                BuildDefaultRoster(rosterParent);
        }

        void ApplyCameraFromLayout()
        {
            var cameraSetup = FindFirstObjectByType<TopDownCameraSetup>();
            if (cameraSetup == null)
                cameraSetup = gameObject.AddComponent<TopDownCameraSetup>();

            cameraSetup.position = new Vector3(floorLayout.cameraPosition.x, floorLayout.cameraPosition.y, -10f);
            cameraSetup.orthographicSize = floorLayout.cameraOrthographicSize;

            var camera = Camera.main;
            if (camera == null)
            {
                var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
                cameraObject.tag = "MainCamera";
                camera = cameraObject.GetComponent<Camera>();
            }
            else if (camera.GetComponent<AudioListener>() == null)
            {
                camera.gameObject.AddComponent<AudioListener>();
            }

            camera.gameObject.SetActive(true);
            camera.orthographic = true;
            camera.orthographicSize = floorLayout.cameraOrthographicSize;
            camera.transform.position = cameraSetup.position;
        }

        void CreateBackgroundFromLayout()
        {
            if (floorLayout.backgroundSprite == null)
                return;

            var backgroundObject = new GameObject("WorkFloorBackground", typeof(SpriteRenderer));
            backgroundObject.transform.SetParent(workRoot, false);
            backgroundObject.transform.position = new Vector3(
                floorLayout.backgroundOrigin.x + floorLayout.worldSize.x * 0.5f,
                floorLayout.backgroundOrigin.y + floorLayout.worldSize.y * 0.5f,
                0f);

            var sprite = backgroundObject.GetComponent<SpriteRenderer>();
            sprite.sprite = floorLayout.backgroundSprite;
            sprite.sortingOrder = 0;

            var spriteSize = floorLayout.backgroundSprite.bounds.size;
            if (spriteSize.x > 0f && spriteSize.y > 0f)
            {
                backgroundObject.transform.localScale = new Vector3(
                    floorLayout.worldSize.x / spriteSize.x,
                    floorLayout.worldSize.y / spriteSize.y,
                    1f);
            }
        }

        WorkStation[] BuildStationsFromLayout(Transform parent)
        {
            var stations = new WorkStation[floorLayout.stations.Length];
            for (var i = 0; i < floorLayout.stations.Length; i++)
            {
                var definition = floorLayout.stations[i];
                var position = floorLayout.NormalizedToWorld(definition.normalizedPosition);
                stations[i] = CreateStationFromDefinition(parent, definition, position);
            }
            return stations;
        }

        WorkStation[] BuildDefaultStations(Transform parent)
        {
            var stationA = CreateStationFromDefinition(parent, DefaultPermanent("Builder Bay", WorkerRole.Builder, new Color(0.55f, 0.3f, 0.2f, 1f), 2), new Vector3(-2f, 2f, 0f));
            var stationB = CreateStationFromDefinition(parent, DefaultPermanent("Analysis Desk", WorkerRole.Analyst, new Color(0.2f, 0.35f, 0.6f, 1f), 2), new Vector3(2f, 2f, 0f));
            var stationC = CreateStationFromDefinition(parent, DefaultTimed("Rush Courier Job", WorkerRole.Courier, new Color(0.2f, 0.5f, 0.25f, 1f), 10f, 60f, 10f, 1, 10), new Vector3(-2f, -2f, 0f));
            var stationD = CreateStationFromDefinition(parent, DefaultTimed("Emergency Build", WorkerRole.Builder, new Color(0.75f, 0.45f, 0.2f, 1f), 15f, 55f, 14f, 1, 12), new Vector3(2f, -2f, 0f));
            CreateLabel(parent, "Work Stations", new Vector3(0f, 3.5f, 0f));
            return new[] { stationA, stationB, stationC, stationD };
        }

        void BuildRosterFromLayout(Transform parent)
        {
            for (var i = 0; i < floorLayout.rosterSlots.Length; i++)
            {
                var slot = floorLayout.rosterSlots[i];
                CreateWorker(parent, slot.role, floorLayout.NormalizedToWorld(slot.normalizedPosition), slot.displayName);
            }
        }

        void BuildDefaultRoster(Transform parent)
        {
            var rosterPositions = new[]
            {
                new Vector3(-4.5f, -3.85f, 0f),
                new Vector3(-1.5f, -3.85f, 0f),
                new Vector3(1.5f, -3.85f, 0f),
                new Vector3(4.5f, -3.85f, 0f)
            };

            var roles = new[]
            {
                WorkerRole.Analyst,
                WorkerRole.Builder,
                WorkerRole.Courier,
                WorkerRole.Builder
            };

            var names = new[]
            {
                "Dad",
                "Mom",
                "Mia",
                "Leo"
            };

            for (var i = 0; i < rosterPositions.Length; i++)
                CreateWorker(parent, roles[i], rosterPositions[i], names[i]);
        }

        static WorkStationDefinition DefaultPermanent(string label, WorkerRole role, Color color, int capacity)
        {
            return new WorkStationDefinition
            {
                stationId = label,
                displayLabel = label,
                requiredRole = role,
                tint = color,
                capacity = capacity,
                outputPerWorker = 3f,
                mode = WorkStationMode.PermanentProduction
            };
        }

        static WorkStationDefinition DefaultTimed(string label, WorkerRole role, Color color, float spawnStart, float spawnEnd, float duration, int capacity, int reward)
        {
            return new WorkStationDefinition
            {
                stationId = label,
                displayLabel = label,
                requiredRole = role,
                tint = color,
                capacity = capacity,
                mode = WorkStationMode.TimedTask,
                spawnWindowStart = spawnStart,
                spawnWindowEnd = spawnEnd,
                taskDuration = duration,
                taskOutputReward = reward,
                taskProgressPerWorker = 1.2f,
                correctWorkerSpeedMultiplier = 2f,
                activeTaskRoundTimeBonus = 0.35f
            };
        }

        WorkStation CreateStationFromDefinition(Transform parent, WorkStationDefinition definition, Vector3 position)
        {
            var label = string.IsNullOrEmpty(definition.displayLabel) ? definition.stationId : definition.displayLabel;
            var stationObject = new GameObject(label, typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(WorkStation));
            stationObject.transform.SetParent(parent, false);
            stationObject.transform.position = position;
            stationObject.transform.localScale = Vector3.one;

            var overlayAlpha = floorLayout != null ? floorLayout.stationOverlayAlpha : 1f;
            var showOverlay = floorLayout == null || floorLayout.showStationOverlay;
            var stationColor = definition.tint;
            if (showOverlay && floorLayout != null)
                stationColor.a = overlayAlpha;

            var sprite = stationObject.GetComponent<SpriteRenderer>();
            sprite.sprite = squareSprite;
            sprite.color = showOverlay ? stationColor : new Color(stationColor.r, stationColor.g, stationColor.b, 0f);
            sprite.sortingOrder = 2;
            sprite.transform.localScale = new Vector3(definition.visualScale.x, definition.visualScale.y, 1f);

            var collider = stationObject.GetComponent<BoxCollider2D>();
            collider.isTrigger = false;
            collider.size = definition.colliderSize;

            var station = stationObject.GetComponent<WorkStation>();
            station.stationId = definition.stationId;
            station.requiredRole = definition.requiredRole;
            station.acceptAnyMember = definition.acceptAnyMember;
            station.allowedMemberColors = definition.allowedMemberColors != WorkerColor.None
                ? definition.allowedMemberColors
                : WorkerColorRules.FromRole(definition.requiredRole);
            station.mode = definition.mode;
            station.capacity = definition.capacity;
            station.outputPerWorker = definition.outputPerWorker;
            station.spawnWindowStart = definition.spawnWindowStart;
            station.spawnWindowEnd = definition.spawnWindowEnd;
            station.taskDuration = definition.taskDuration;
            station.taskProgressPerWorker = definition.taskProgressPerWorker;
            station.correctWorkerSpeedMultiplier = definition.correctWorkerSpeedMultiplier;
            station.activeTaskRoundTimeBonus = definition.activeTaskRoundTimeBonus;
            station.taskOutputReward = definition.taskOutputReward;

            if (definition.mode == WorkStationMode.TimedTask)
                stationObject.SetActive(false);

            if (showOverlay)
                CreateLabel(stationObject.transform, label, new Vector3(0f, 0.55f, 0f));

            return station;
        }

        void CreateWorker(Transform parent, WorkerRole role, Vector3 homePosition, string workerName = null)
        {
            var workerObject = new GameObject($"Worker_{workerName ?? role.ToString()}", typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(WorkerUnit));
            workerObject.transform.SetParent(parent, false);
            workerObject.transform.localScale = Vector3.one;

            var sprite = workerObject.GetComponent<SpriteRenderer>();
            sprite.sprite = squareSprite;
            sprite.sortingOrder = 10;

            var collider = workerObject.GetComponent<BoxCollider2D>();
            collider.size = Vector2.one;

            var worker = workerObject.GetComponent<WorkerUnit>();
            worker.Initialize(role, homePosition, workerName);
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
            {
                var canvasObject = GameObject.Find("UI Canvas");
                if (canvasObject != null)
                    mainCanvas = canvasObject.GetComponent<Canvas>();
            }

            if (mainCanvas == null)
                mainCanvas = FindAnyObjectByType<Canvas>();

            if (mainCanvas != null)
                mainCanvas.gameObject.SetActive(true);

            EnsureGameplayHUD();
            if (mainCanvas != null)
                UIFontProvider.ApplyToHierarchy(mainCanvas.transform);
            HideMenuPanels();
            EnsureStartPanel();
        }

        void EnsureGameplayHUD()
        {
            if (FindFirstObjectByType<GameplayHUDView>() != null)
                return;

            var canvas = mainCanvas != null ? mainCanvas : FindFirstObjectByType<Canvas>();
            if (canvas == null)
                return;

            if (gameplayHudPrefab == null)
            {
#if UNITY_EDITOR
                gameplayHudPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>("Assets/UI/prefab/GameplayHUD_p.prefab");
#endif
            }

            if (gameplayHudPrefab == null)
                return;

            var hudInstance = Instantiate(gameplayHudPrefab, canvas.transform);
            hudInstance.name = "GameplayHUD";
            StripNestedCanvas(hudInstance);
            StretchToParent(hudInstance);
            UIFontProvider.ApplyToHierarchy(hudInstance.transform);
        }

        static void StripNestedCanvas(GameObject hudInstance)
        {
            var nestedCanvas = hudInstance.GetComponent<Canvas>();
            if (nestedCanvas != null)
                Destroy(nestedCanvas);

            var scaler = hudInstance.GetComponent<CanvasScaler>();
            if (scaler != null)
                Destroy(scaler);

            var raycaster = hudInstance.GetComponent<GraphicRaycaster>();
            if (raycaster != null)
                Destroy(raycaster);
        }

        static void StretchToParent(GameObject hudInstance)
        {
            var rect = hudInstance.GetComponent<RectTransform>();
            if (rect == null)
                return;

            rect.localScale = Vector3.one;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        void HideMenuPanels()
        {
            var metaGame = GetComponent<MetaGameController>();
            if (metaGame != null)
            {
                metaGame.TogglePauseMenu(false);
                return;
            }

            if (mainCanvas == null)
                return;

            var menu = mainCanvas.GetComponent<MainUIController>();
            if (menu != null)
                menu.HideAllPanels();
        }

        void EnsureStartPanel()
        {
            if (autoStartOnLoad)
                return;

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
            UIFontProvider.Apply(label);
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
