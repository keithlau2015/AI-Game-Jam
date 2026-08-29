using Platformer.Core;
using Platformer.Mechanics;
using Platformer.Model;

namespace Platformer.Gameplay
{
    public class RoundLost : Simulation.Event<RoundLost>
    {
        SessionModel session = Simulation.GetModel<SessionModel>();

        public override void Execute()
        {
            session.round.phase = RoundPhase.Lost;
            session.round.dragEnabled = false;

            if (GameAudioController.Instance != null)
                GameAudioController.Instance.PlayRoundLose();
        }
    }
}
