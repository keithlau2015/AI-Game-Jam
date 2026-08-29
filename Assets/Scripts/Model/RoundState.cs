using System;
using UnityEngine;

namespace Platformer.Model
{
    [Serializable]
    public class RoundState
    {
        public float timeLimit = 90f;
        public float timeRemaining;
        public int targetOutput = 50;
        public int currentOutput;
        public float globalProductionMultiplier = 1f;
        public RoundPhase phase = RoundPhase.Idle;
        public bool dragEnabled = true;
    }
}
