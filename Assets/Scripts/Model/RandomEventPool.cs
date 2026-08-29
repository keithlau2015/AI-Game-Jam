using UnityEngine;

namespace Platformer.Model
{
    [CreateAssetMenu(fileName = "RandomEventPool", menuName = "Platformer/Random Event Pool")]
    public class RandomEventPool : ScriptableObject
    {
        public RandomEventDefinition[] events;
        public bool triggerOnTimer = true;
        public float intervalMin = 25f;
        public float intervalMax = 45f;
        public float initialDelay = 10f;
    }
}
