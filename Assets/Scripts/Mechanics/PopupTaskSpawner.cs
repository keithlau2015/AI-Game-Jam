using System.Collections.Generic;
using Platformer.Core;
using Platformer.Model;
using TMPro;
using UnityEngine;

namespace Platformer.Mechanics
{
    public class PopupTaskSpawner : MonoBehaviour
    {
        public PopupTaskPool taskPool;
        public WorkFloorLayout floorLayout;
        public Transform popupRoot;
        public int maxActiveTasks = 3;

        readonly List<PopupTaskBehaviour> activeTasks = new List<PopupTaskBehaviour>();
        readonly List<PopupTaskDefinition> spawnQueue = new List<PopupTaskDefinition>();
        readonly HashSet<string> spawnedThisRound = new HashSet<string>();

        SessionModel session;
        float nextSpawnAllowedTime;
        static Sprite squareSprite;

        void Awake()
        {
            session = Simulation.GetModel<SessionModel>();
            if (popupRoot == null)
            {
                var rootObject = new GameObject("PopupTasks");
                rootObject.transform.SetParent(transform, false);
                popupRoot = rootObject.transform;
            }
        }

        public void PrepareForRound()
        {
            ClearActiveTasks();
            spawnQueue.Clear();
            spawnedThisRound.Clear();
            nextSpawnAllowedTime = 0f;

            if (taskPool == null)
                return;

            var tasks = taskPool.Tasks;
            for (var i = 0; i < tasks.Count; i++)
            {
                if (tasks[i] != null)
                    spawnQueue.Add(tasks[i]);
            }

            Shuffle(spawnQueue);
        }

        public void Tick(float deltaTime, float elapsedRoundTime)
        {
            if (taskPool == null || session == null)
                return;
            if (session.round.phase != RoundPhase.Playing || session.eventState.awaitingDecision)
                return;

            CleanupInactiveTasks();

            for (var i = activeTasks.Count - 1; i >= 0; i--)
            {
                if (activeTasks[i] != null)
                    activeTasks[i].TickExistence(deltaTime);
            }

            if (activeTasks.Count >= maxActiveTasks)
                return;
            if (elapsedRoundTime < nextSpawnAllowedTime)
                return;

            for (var i = 0; i < spawnQueue.Count; i++)
            {
                var definition = spawnQueue[i];
                if (definition == null || spawnedThisRound.Contains(definition.taskId))
                    continue;
                if (elapsedRoundTime < definition.spawnTimeMin || elapsedRoundTime > definition.spawnTimeMax)
                    continue;

                SpawnTask(definition);
                spawnedThisRound.Add(definition.taskId);
                nextSpawnAllowedTime = elapsedRoundTime + taskPool.minGapBetweenSpawns;
                break;
            }
        }

        void SpawnTask(PopupTaskDefinition definition)
        {
            EnsureSquareSprite();
            var position = PickSpawnPosition();
            var taskObject = new GameObject(
                $"Popup_{definition.taskId}",
                typeof(SpriteRenderer),
                typeof(BoxCollider2D),
                typeof(WorkStation),
                typeof(PopupTaskBehaviour));

            taskObject.transform.SetParent(popupRoot, false);
            taskObject.transform.position = position;

            var sprite = taskObject.GetComponent<SpriteRenderer>();
            sprite.sprite = squareSprite;
            sprite.color = new Color(0.95f, 0.55f, 0.2f, 0.92f);
            sprite.sortingOrder = 6;
            sprite.transform.localScale = new Vector3(2.2f, 1.6f, 1f);

            var collider = taskObject.GetComponent<BoxCollider2D>();
            collider.size = new Vector2(2.2f, 1.6f);

            var behaviour = taskObject.GetComponent<PopupTaskBehaviour>();
            behaviour.Configure(definition);

            var station = taskObject.GetComponent<WorkStation>();
            station.PrepareForRound();
            station.ForceActivatePopup();

            if (RoundController.Instance != null)
                RoundController.Instance.RegisterStation(station);

            activeTasks.Add(behaviour);
            CreateTitleLabel(taskObject.transform, definition.title);
        }

        void CreateTitleLabel(Transform parent, string title)
        {
            var labelObject = new GameObject("PopupTitle", typeof(TextMeshPro));
            labelObject.transform.SetParent(parent, false);
            labelObject.transform.localPosition = new Vector3(0f, -0.75f, 0f);
            var label = labelObject.GetComponent<TextMeshPro>();
            label.text = title;
            label.fontSize = 1.8f;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
        }

        Vector3 PickSpawnPosition()
        {
            if (floorLayout != null && taskPool != null)
            {
                var min = taskPool.spawnNormalizedMin;
                var max = taskPool.spawnNormalizedMax;
                var normalized = new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));
                return floorLayout.NormalizedToWorld(normalized);
            }

            return new Vector3(Random.Range(-5f, 5f), Random.Range(-2f, 3f), 0f);
        }

        void CleanupInactiveTasks()
        {
            for (var i = activeTasks.Count - 1; i >= 0; i--)
            {
                var task = activeTasks[i];
                if (task != null && task.gameObject.activeInHierarchy)
                    continue;

                if (task != null)
                {
                    var station = task.GetComponent<WorkStation>();
                    if (station != null && RoundController.Instance != null)
                        RoundController.Instance.UnregisterStation(station);
                    Destroy(task.gameObject);
                }

                activeTasks.RemoveAt(i);
            }
        }

        void ClearActiveTasks()
        {
            for (var i = activeTasks.Count - 1; i >= 0; i--)
            {
                if (activeTasks[i] == null)
                    continue;
                var station = activeTasks[i].GetComponent<WorkStation>();
                if (station != null && RoundController.Instance != null)
                    RoundController.Instance.UnregisterStation(station);
                Destroy(activeTasks[i].gameObject);
            }
            activeTasks.Clear();
        }

        static void Shuffle<T>(IList<T> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var swapIndex = Random.Range(0, i + 1);
                (list[i], list[swapIndex]) = (list[swapIndex], list[i]);
            }
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
