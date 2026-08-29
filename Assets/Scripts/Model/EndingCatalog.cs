using Platformer.Mechanics;
using UnityEngine;

namespace Platformer.Model
{
    [CreateAssetMenu(fileName = "EndingCatalog", menuName = "Platformer/Ending Catalog")]
    public class EndingCatalog : ScriptableObject
    {
        public Sprite defaultImage;
        public EndingDefinition defeatEnding;
        public EndingDefinition timeExpiredEnding;
        public EndingDefinition statLossEnding;
        public EndingDefinition campaignCompleteEnding;
        public EndingDefinition builderFocusedEnding;
        public EndingDefinition analystFocusedEnding;
        public EndingDefinition courierFocusedEnding;
        public EndingDefinition balancedEnding;

        public EndingDefinition Resolve(SessionModel session, WorkerUnit[] workers, bool won)
        {
            if (!won)
                return ResolveDefeat(session);

            if (session.currentDay >= SessionModel.TotalDays)
                return ResolveCampaignComplete(workers);

            return ResolveDayVictory(workers);
        }

        EndingDefinition ResolveDefeat(SessionModel session)
        {
            return session.lastEndReason switch
            {
                RoundEndReason.StatMinReached when statLossEnding != null => statLossEnding,
                _ => defeatEnding
            };
        }

        EndingDefinition ResolveDayVictory(WorkerUnit[] workers)
        {
            return ResolveByAttributes(workers);
        }

        EndingDefinition ResolveCampaignComplete(WorkerUnit[] workers)
        {
            if (campaignCompleteEnding != null)
                return campaignCompleteEnding;

            return ResolveByAttributes(workers);
        }

        EndingDefinition ResolveByAttributes(WorkerUnit[] workers)
        {
            if (workers == null || workers.Length == 0)
                return balancedEnding != null ? balancedEnding : defeatEnding;

            var builderTotal = 0;
            var analystTotal = 0;
            var courierTotal = 0;

            for (var i = 0; i < workers.Length; i++)
            {
                if (workers[i] == null)
                    continue;

                builderTotal += workers[i].attributes.builderSkill;
                analystTotal += workers[i].attributes.analystSkill;
                courierTotal += workers[i].attributes.courierSkill;
            }

            var count = workers.Length;
            var builderAvg = builderTotal / (float)count;
            var analystAvg = analystTotal / (float)count;
            var courierAvg = courierTotal / (float)count;
            var max = Mathf.Max(builderAvg, Mathf.Max(analystAvg, courierAvg));
            var min = Mathf.Min(builderAvg, Mathf.Min(analystAvg, courierAvg));

            if (max - min <= 8f && balancedEnding != null)
                return balancedEnding;

            if (builderAvg >= analystAvg && builderAvg >= courierAvg && builderFocusedEnding != null)
                return builderFocusedEnding;

            if (analystAvg >= builderAvg && analystAvg >= courierAvg && analystFocusedEnding != null)
                return analystFocusedEnding;

            if (courierFocusedEnding != null)
                return courierFocusedEnding;

            return balancedEnding != null ? balancedEnding : defeatEnding;
        }

        public Sprite ResolveImage(EndingDefinition ending)
        {
            if (ending != null && ending.image != null)
                return ending.image;

            return defaultImage;
        }
    }
}
