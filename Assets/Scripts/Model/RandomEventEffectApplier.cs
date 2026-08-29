using Platformer.Mechanics;
using UnityEngine;

namespace Platformer.Model
{
    public static class RandomEventEffectApplier
    {
        public static void Apply(RandomEventOption option, SessionModel session)
        {
            if (option == null || session == null)
                return;

            ApplySingle(option.effectType, option.value, option.duration, session);
            if (option.secondaryEffectType != RandomEventEffectType.None)
                ApplySingle(option.secondaryEffectType, option.secondaryValue, option.duration, session);
        }

        static void ApplySingle(RandomEventEffectType effectType, float value, float duration, SessionModel session)
        {
            switch (effectType)
            {
                case RandomEventEffectType.None:
                case RandomEventEffectType.NarrativeOnly:
                    break;
                case RandomEventEffectType.AddHope:
                    session.hope += Mathf.RoundToInt(value);
                    session.ClampStats();
                    break;
                case RandomEventEffectType.ModifyStress:
                    session.stress += Mathf.RoundToInt(value);
                    session.ClampStats();
                    break;
                case RandomEventEffectType.ModifyRapport:
                    session.rapport += Mathf.RoundToInt(value);
                    session.ClampStats();
                    break;
                case RandomEventEffectType.AddTime:
                    if (RoundController.Instance != null)
                        RoundController.Instance.AddTime(value);
                    else
                        session.round.timeRemaining += value;
                    break;
                case RandomEventEffectType.RemoveTime:
                    session.round.timeRemaining = Mathf.Max(0f, session.round.timeRemaining - value);
                    break;
                case RandomEventEffectType.ModifyProductionRate:
                    if (RoundController.Instance != null)
                        RoundController.Instance.ModifyProductionMultiplier(value);
                    else
                        session.round.globalProductionMultiplier = Mathf.Max(0.1f, session.round.globalProductionMultiplier + value);
                    break;
                case RandomEventEffectType.DisableRandomStation:
                    if (RoundController.Instance != null)
                        RoundController.Instance.DisableRandomStation();
                    break;
                case RandomEventEffectType.ModifyTimeScale:
                    Time.timeScale = Mathf.Max(0.1f, value);
                    break;
            }
        }
    }
}
