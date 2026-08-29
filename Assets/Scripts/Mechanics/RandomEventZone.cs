using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Model;
using UnityEngine;
using static Platformer.Core.Simulation;

namespace Platformer.Mechanics
{
    public class RandomEventZone : MonoBehaviour
    {
        public RandomEventPool overridePool;
        public bool triggerOnce = true;

        bool triggered;
        SessionModel session;

        void Awake()
        {
            session = Simulation.GetModel<SessionModel>();
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            if (triggerOnce && triggered)
                return;

            var worker = collider.GetComponent<WorkerUnit>();
            if (worker == null)
                return;

            var controller = FindFirstObjectByType<RandomEventController>();
            if (controller == null)
                return;

            if (overridePool != null)
                controller.eventPool = overridePool;

            controller.TryTriggerRandomEvent();
            triggered = true;
        }
    }
}
