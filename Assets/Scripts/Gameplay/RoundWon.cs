using Platformer.Core;
using Platformer.Model;

namespace Platformer.Gameplay
{
    public class RoundWon : Simulation.Event<RoundWon>
    {
        SessionModel session = Simulation.GetModel<SessionModel>();

        public override void Execute()
        {
            session.round.phase = RoundPhase.Won;
            session.round.dragEnabled = false;
        }
    }
}
