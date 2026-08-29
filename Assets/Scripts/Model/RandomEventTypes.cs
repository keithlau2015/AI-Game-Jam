using System;
using System.Collections.Generic;
using UnityEngine;

namespace Platformer.Model
{
    public enum RandomEventEffectType
    {
        None,
        ModifyHealth,
        SetJumpModifier,
        AddJumpModifier,
        SetMaxSpeed,
        AddMaxSpeed,
        SetGravityModifier,
        AddGravityModifier,
        ModifyTimeScale,
        TeleportToSpawn,
        ForceDeath,
        InvertControls,
        DisableJump,
        EnableJump,
        AddHope,
        ModifyStress,
        ModifyRapport,
        AddTime,
        RemoveTime,
        ModifyProductionRate,
        DisableRandomStation,
        NarrativeOnly
    }

    [Serializable]
    public class RandomEventOption
    {
        public string label;
        [TextArea(2, 5)]
        public string outcomeText;
        public RandomEventEffectType effectType;
        public float value;
        public float duration;
        public RandomEventEffectType secondaryEffectType;
        public float secondaryValue;
        public RandomEventEffectType tertiaryEffectType;
        public float tertiaryValue;
    }

    [Serializable]
    public class RandomEventState
    {
        public RandomEventDefinition pendingEvent;
        public bool awaitingDecision;
        public int karma;
        public readonly HashSet<RandomEventDefinition> playedOnce = new HashSet<RandomEventDefinition>();
    }
}
