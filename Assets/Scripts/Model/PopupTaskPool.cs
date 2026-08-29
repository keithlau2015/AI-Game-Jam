using System.Collections.Generic;
using UnityEngine;

namespace Platformer.Model
{
    [CreateAssetMenu(fileName = "PopupTaskPool", menuName = "Platformer/Popup Task Pool")]
    public class PopupTaskPool : ScriptableObject
    {
        public TextAsset csvSource;
        public int maxConcurrentTasks = 3;
        public float minGapBetweenSpawns = 8f;
        public Vector2 spawnNormalizedMin = new Vector2(0.12f, 0.22f);
        public Vector2 spawnNormalizedMax = new Vector2(0.88f, 0.78f);

        [SerializeField] List<PopupTaskDefinition> cachedTasks = new List<PopupTaskDefinition>();

        public IReadOnlyList<PopupTaskDefinition> Tasks
        {
            get
            {
                if (cachedTasks == null || cachedTasks.Count == 0)
                    ReloadFromCsv();
                return cachedTasks;
            }
        }

        public void ReloadFromCsv()
        {
            cachedTasks = csvSource != null
                ? PopupTaskCsvLoader.Load(csvSource)
                : new List<PopupTaskDefinition>();
        }

        void OnValidate()
        {
            if (csvSource != null)
                ReloadFromCsv();
        }
    }
}
