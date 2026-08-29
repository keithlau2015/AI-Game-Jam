using System;
using UnityEngine;

namespace Platformer.Model
{
    public enum PopupStatTarget
    {
        Hope,
        Stress,
        Rapport,
        Happiness
    }

    [Serializable]
    public struct PopupTaskEffect
    {
        public PopupStatTarget target;
        public FamilyMemberId member;
        public int delta;
    }

    [Serializable]
    public class PopupTaskRequirement
    {
        public int minParticipants = 1;
        public int maxParticipants = 1;
        public bool requireAllFamily;
        public FamilyMemberId forbiddenMask;
        public FamilyMemberId[] requiredGroups = Array.Empty<FamilyMemberId>();
    }

    [Serializable]
    public class PopupTaskDefinition
    {
        public string taskId;
        public string title;
        public string icon;
        public int maxParticipants = 1;
        public string roleRequirementRaw;
        public PopupTaskRequirement requirement = new PopupTaskRequirement();
        public float existenceDuration = 15f;
        public float spawnTimeMin = 10f;
        public float spawnTimeMax = 60f;
        public float workDuration = 8f;
        public string successOutcomeRaw;
        public string failureOutcomeRaw;
        public PopupTaskEffect[] successEffects = Array.Empty<PopupTaskEffect>();
        public PopupTaskEffect[] failureEffects = Array.Empty<PopupTaskEffect>();
    }
}
