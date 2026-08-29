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
        public int hope = 50;
        public int stress = 50;
        public int rapport = 50;
        public int eventsResolved;
        public bool sessionStarted;
        public const int TotalDays = 4;
        public int currentDay = 1;
        public RoundEndReason lastEndReason = RoundEndReason.None;

        public void ClampStats()
        {
            hope = Mathf.Clamp(hope, statMin, statMax);
            stress = Mathf.Clamp(stress, statMin, statMax);
            rapport = Mathf.Clamp(rapport, statMin, statMax);
        }

        public bool HasStatLoss()
        {
            return hope <= statMin || stress <= statMin || rapport <= statMin;
        }
    }
}
