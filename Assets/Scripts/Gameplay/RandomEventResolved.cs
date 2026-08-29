using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    public class RandomEventResolved : Simulation.Event<RandomEventResolved>
    {
        SessionModel session = Simulation.GetModel<SessionModel>();

        public RandomEventDefinition definition;
        public int optionIndex;

        public override bool Precondition()
        {
            return definition != null
                && definition.options != null
                && optionIndex >= 0
                && optionIndex < definition.options.Length;
        }

        public override void Execute()
        {
            var option = definition.options[optionIndex];
            RandomEventEffectApplier.Apply(option, session);
            SessionEndChecker.CheckAndSchedule(session);

            session.eventState.pendingEvent = null;
            session.eventState.awaitingDecision = false;
            session.eventsResolved += 1;

            if (Time.timeScale == 0f)
                Time.timeScale = 1f;

            session.round.dragEnabled = true;
            if (RoundController.Instance != null)
                RoundController.Instance.SetPausedForEvent(false);
        }

        internal override void Cleanup()
        {
            definition = null;
            optionIndex = 0;
        }
    }
}
