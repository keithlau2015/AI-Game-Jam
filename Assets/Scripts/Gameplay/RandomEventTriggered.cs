using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    public class RandomEventTriggered : Simulation.Event<RandomEventTriggered>
    {
        SessionModel session = Simulation.GetModel<SessionModel>();

        public RandomEventDefinition definition;

        public override bool Precondition()
        {
            return definition != null && !session.eventState.awaitingDecision;
        }

        public override void Execute()
        {
            session.eventState.pendingEvent = definition;
            session.eventState.awaitingDecision = true;

            if (!definition.canRepeat)
                session.eventState.playedOnce.Add(definition);

            session.round.dragEnabled = false;
            if (RoundController.Instance != null)
                RoundController.Instance.SetPausedForEvent(true);

            Time.timeScale = 0f;
        }

        internal override void Cleanup()
        {
            definition = null;
        }
    }
}
