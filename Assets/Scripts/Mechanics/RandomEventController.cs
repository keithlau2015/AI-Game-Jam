using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Model;
using UnityEngine;

namespace Platformer.Mechanics
{
    public class RandomEventController : MonoBehaviour
    {
        public RandomEventPool eventPool;

        SessionModel session;
        float nextTriggerTime;
        bool sessionActive;

        void OnEnable()
        {
            session = Simulation.GetModel<SessionModel>();
            if (eventPool == null)
                eventPool = session.eventPool;
        }

        void Update()
        {
            if (!sessionActive || eventPool == null || !eventPool.triggerOnTimer)
                return;
            if (session.eventState.awaitingDecision)
                return;
            if (session.round.phase != RoundPhase.Playing)
                return;
            if (Time.time < nextTriggerTime)
                return;

            TryTriggerRandomEvent();
            ScheduleNextTrigger(Random.Range(eventPool.intervalMin, eventPool.intervalMax));
        }

        public void StartSession()
        {
            sessionActive = true;
            ScheduleNextTrigger(eventPool != null ? eventPool.initialDelay : 15f);
        }

        public void TryTriggerRandomEvent()
        {
            if (eventPool == null || session.eventState.awaitingDecision)
                return;

            var definition = RandomEventSelector.Pick(eventPool, session.eventState.playedOnce);
            if (definition == null)
                return;

            var ev = Simulation.Schedule<RandomEventTriggered>();
            ev.definition = definition;
        }

        void ScheduleNextTrigger(float delay)
        {
            nextTriggerTime = Time.time + Mathf.Max(0f, delay);
        }
    }
}
