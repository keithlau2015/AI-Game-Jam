using System.Collections.Generic;
using UnityEngine;

namespace Platformer.Model
{
    public static class RandomEventSelector
    {
        public static RandomEventDefinition Pick(RandomEventPool pool, HashSet<RandomEventDefinition> playedOnce)
        {
            if (pool == null || pool.events == null || pool.events.Length == 0)
                return null;

            var totalWeight = 0;
            for (var i = 0; i < pool.events.Length; i++)
            {
                var definition = pool.events[i];
                if (!IsEligible(definition, playedOnce))
                    continue;
                totalWeight += Mathf.Max(1, definition.weight);
            }

            if (totalWeight <= 0)
                return null;

            var roll = Random.Range(0, totalWeight);
            for (var i = 0; i < pool.events.Length; i++)
            {
                var definition = pool.events[i];
                if (!IsEligible(definition, playedOnce))
                    continue;

                roll -= Mathf.Max(1, definition.weight);
                if (roll < 0)
                    return definition;
            }

            return null;
        }

        static bool IsEligible(RandomEventDefinition definition, HashSet<RandomEventDefinition> playedOnce)
        {
            if (definition == null)
                return false;
            if (definition.options == null || definition.options.Length == 0)
                return false;
            if (!definition.canRepeat && playedOnce != null && playedOnce.Contains(definition))
                return false;
            return true;
        }
    }
}
