using UnityEngine;

namespace Platformer.Model
{
    [CreateAssetMenu(fileName = "Ending", menuName = "Platformer/Ending Definition")]
    public class EndingDefinition : ScriptableObject
    {
        public string title;
        [TextArea(2, 6)]
        public string description;
        public Sprite image;
    }
}
