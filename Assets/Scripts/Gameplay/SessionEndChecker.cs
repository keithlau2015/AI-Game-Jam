using Platformer.Core;
using Platformer.Model;

namespace Platformer.Gameplay
{
    public static class SessionEndChecker
    {
        public static void CheckAndSchedule(SessionModel session)
        {
            if (session == null)
                return;
            if (session.round.phase == RoundPhase.Won || session.round.phase == RoundPhase.Lost)
                return;

            session.ClampStats();

            if (session.HasStatLoss())
            {
                session.lastEndReason = RoundEndReason.StatMinReached;
                Simulation.Schedule<RoundLost>();
            }
        }
    }
}
