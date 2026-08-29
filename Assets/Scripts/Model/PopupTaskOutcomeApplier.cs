using Platformer.Mechanics;
using UnityEngine;

namespace Platformer.Model
{
    public static class PopupTaskOutcomeApplier
    {
        public static void Apply(PopupTaskEffect[] effects, SessionModel session, WorkerUnit[] participants)
        {
            if (effects == null || session == null)
                return;

            for (var i = 0; i < effects.Length; i++)
                ApplySingle(effects[i], session, participants);

            session.ClampStats();
            SessionEndCheckerBridge.Check(session);
        }

        static void ApplySingle(PopupTaskEffect effect, SessionModel session, WorkerUnit[] participants)
        {
            switch (effect.target)
            {
                case PopupStatTarget.Hope:
                    session.hope += effect.delta;
                    break;
                case PopupStatTarget.Stress:
                    session.stress += effect.delta;
                    break;
                case PopupStatTarget.Rapport:
                    session.rapport += effect.delta;
                    break;
                case PopupStatTarget.Happiness:
                    ApplyHappiness(effect, participants);
                    break;
            }
        }

        static void ApplyHappiness(PopupTaskEffect effect, WorkerUnit[] participants)
        {
            if (participants == null)
                return;

            for (var i = 0; i < participants.Length; i++)
            {
                var worker = participants[i];
                if (worker == null)
                    continue;
                if (effect.member != FamilyMemberId.None
                    && effect.member != FamilyMemberId.All
                    && (effect.member & worker.familyMember) == 0)
                    continue;

                var attributes = worker.attributes;
                attributes.happiness = Mathf.Clamp(attributes.happiness + effect.delta, 0, 100);
                worker.attributes = attributes;
            }
        }
    }

    static class SessionEndCheckerBridge
    {
        public static void Check(SessionModel session)
        {
            Platformer.Gameplay.SessionEndChecker.CheckAndSchedule(session);
        }
    }
}
