using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Gameplay
{
    public static class RandomEventFlow
    {
        public static void CompleteOutcomeAcknowledgement(SessionModel session)
        {
            if (session == null)
                return;

            session.eventState.awaitingDecision = false;

            if (Time.timeScale == 0f)
                Time.timeScale = 1f;

            session.round.dragEnabled = true;
            if (RoundController.Instance != null)
                RoundController.Instance.SetPausedForEvent(false);
        }
    }
}
