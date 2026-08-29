using System.Collections.Generic;
using UnityEngine;

namespace Platformer.Model
{
    public static class RandomEventEffectPresentation
    {
        public static string BuildPreviewLabel(RandomEventOption option)
        {
            if (option == null)
                return string.Empty;

            var stats = CollectGlobalStatNames(option);
            if (stats.Count == 0)
                return string.Empty;

            return "影響：" + string.Join(" / ", stats);
        }

        public static string BuildResultLabel(RandomEventOption option)
        {
            if (option == null)
                return string.Empty;

            var lines = CollectGlobalStatChanges(option);
            if (lines.Count == 0)
                return "家庭數值沒有變化";

            return string.Join("\n", lines);
        }

        static List<string> CollectGlobalStatNames(RandomEventOption option)
        {
            var names = new List<string>();
            TryAddName(names, option.effectType);
            TryAddName(names, option.secondaryEffectType);
            TryAddName(names, option.tertiaryEffectType);
            return names;
        }

        static List<string> CollectGlobalStatChanges(RandomEventOption option)
        {
            var lines = new List<string>();
            TryAddChange(lines, option.effectType, option.value);
            TryAddChange(lines, option.secondaryEffectType, option.secondaryValue);
            TryAddChange(lines, option.tertiaryEffectType, option.tertiaryValue);
            return lines;
        }

        static void TryAddName(List<string> names, RandomEventEffectType effectType)
        {
            var name = GetGlobalStatName(effectType);
            if (string.IsNullOrEmpty(name) || names.Contains(name))
                return;

            names.Add(name);
        }

        static void TryAddChange(List<string> lines, RandomEventEffectType effectType, float value)
        {
            var name = GetGlobalStatName(effectType);
            if (string.IsNullOrEmpty(name))
                return;

            var delta = Mathf.RoundToInt(value);
            var sign = delta > 0 ? "+" : string.Empty;
            lines.Add($"{name} {sign}{delta}");
        }

        public static string GetGlobalStatName(RandomEventEffectType effectType)
        {
            return effectType switch
            {
                RandomEventEffectType.AddHope => "希望",
                RandomEventEffectType.ModifyStress => "情緒",
                RandomEventEffectType.ModifyRapport => "和諧",
                _ => null
            };
        }
    }
}
