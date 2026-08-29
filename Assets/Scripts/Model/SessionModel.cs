using UnityEngine;

namespace Platformer.Model
{
    [System.Serializable]
    public class SessionModel
    {
        public RandomEventPool eventPool;
        public RandomEventState eventState = new RandomEventState();
        public RoundState round = new RoundState();
        public int statMin;
        public int statMax = 100;
        public int karma = 50;
        public int morale = 50;
        public int reputation = 50;
        public int eventsResolved;
        public bool sessionStarted;

        public void ClampStats()
        {
            karma = Mathf.Clamp(karma, statMin, statMax);
            morale = Mathf.Clamp(morale, statMin, statMax);
            reputation = Mathf.Clamp(reputation, statMin, statMax);
        }

        public bool HasStatWin()
        {
            return karma >= statMax || morale >= statMax || reputation >= statMax;
        }

        public bool HasStatLoss()
        {
            return karma <= statMin || morale <= statMin || reputation <= statMin;
        }
    }
}
