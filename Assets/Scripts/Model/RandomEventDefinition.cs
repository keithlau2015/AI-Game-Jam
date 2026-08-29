using UnityEngine;

namespace Platformer.Model
{
    [CreateAssetMenu(fileName = "RandomEvent", menuName = "Platformer/Random Event")]
    public class RandomEventDefinition : ScriptableObject
    {
        public string title;
        [TextArea(3, 8)]
        public string description;
        public int weight = 1;
        public bool canRepeat = true;
        public RandomEventOption[] options;
    }
}
