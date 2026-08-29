using Platformer.Core;
using Platformer.Mechanics;

namespace Platformer.Gameplay
{
    public class WorkerAssigned : Simulation.Event<WorkerAssigned>
    {
        public WorkerUnit worker;
        public WorkStation station;

        public override void Execute()
        {
            if (GameAudioController.Instance != null)
                GameAudioController.Instance.PlayWorkerAssigned();
        }

        internal override void Cleanup()
        {
            worker = null;
            station = null;
        }
    }
}
