using Platformer.Core;
using Platformer.Model;
using Platformer.UI;
using UnityEngine;

namespace Platformer.Mechanics
{
    public class GameController : MonoBehaviour
    {
        public static GameController Instance { get; private set; }

        public SessionModel session = Simulation.GetModel<SessionModel>();

        void OnEnable()
        {
            Instance = this;
            Simulation.SetModel(session);
            EnsureWorkerSession();
            EnsureRandomEventSystems();
        }

        void EnsureWorkerSession()
        {
            if (GetComponent<WorkSessionBootstrap>() == null)
                gameObject.AddComponent<WorkSessionBootstrap>();
        }

        void EnsureRandomEventSystems()
        {
            var controller = GetComponent<RandomEventController>();
            if (controller == null)
                controller = gameObject.AddComponent<RandomEventController>();

            if (session.eventPool != null)
                controller.eventPool = session.eventPool;
            else if (controller.eventPool != null)
                session.eventPool = controller.eventPool;

            if (FindFirstObjectByType<RandomEventUIController>() == null)
            {
                var uiObject = new GameObject("RandomEventUI");
                uiObject.AddComponent<RandomEventUIController>();
            }
        }

        void OnDisable()
        {
            if (Instance == this) Instance = null;
        }

        void Update()
        {
            if (Instance == this) Simulation.Tick();
        }
    }
}
