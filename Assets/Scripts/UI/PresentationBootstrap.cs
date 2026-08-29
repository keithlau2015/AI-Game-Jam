using Platformer.Mechanics;
using UnityEngine;

namespace Platformer.UI
{
    public class PresentationBootstrap : MonoBehaviour
    {
        public void BeginSession()
        {
            var bootstrap = FindFirstObjectByType<WorkSessionBootstrap>();
            if (bootstrap != null)
            {
                bootstrap.BeginSession();
                return;
            }

            var eventController = FindFirstObjectByType<RandomEventController>();
            if (eventController != null)
                eventController.StartSession();
        }
    }
}
