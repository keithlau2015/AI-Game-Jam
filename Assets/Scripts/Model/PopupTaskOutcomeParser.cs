using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Platformer.Model
{
    public static class PopupTaskOutcomeParser
    {
        static readonly Regex EffectPattern = new Regex(
            @"(?:【(?<scope>[^】]+)】)?(?<stat>希望|压力|壓力|融洽|情绪|情緒)\s*(?<sign>[+\-−－])?\s*(?<value>\d+)",
            RegexOptions.Compiled);

        public static PopupTaskEffect[] Parse(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
                return System.Array.Empty<PopupTaskEffect>();

            var effects = new List<PopupTaskEffect>();
            var matches = EffectPattern.Matches(raw.Replace("\r", string.Empty));
            for (var i = 0; i < matches.Count; i++)
            {
                var match = matches[i];
                if (!match.Success)
                    continue;

                var scope = match.Groups["scope"].Value;
                var stat = match.Groups["stat"].Value;
                var sign = match.Groups["sign"].Value;
                var value = int.Parse(match.Groups["value"].Value);
                if (sign == "-" || sign == "−" || sign == "－")
                    value = -value;

                var target = stat.Contains("希望") ? PopupStatTarget.Hope
                    : stat.Contains("压") || stat.Contains("壓") ? PopupStatTarget.Stress
                    : stat.Contains("融洽") ? PopupStatTarget.Rapport
                    : PopupStatTarget.Happiness;

                var member = ResolveScope(scope, stat);
                effects.Add(new PopupTaskEffect
                {
                    target = target,
                    member = member,
                    delta = value
                });
            }

            return effects.ToArray();
        }

        static FamilyMemberId ResolveScope(string scope, string stat)
        {
            if (string.IsNullOrEmpty(scope))
            {
                if (stat.Contains("爸"))
                    return FamilyMemberId.Dad;
                if (stat.Contains("妈") || stat.Contains("媽"))
                    return FamilyMemberId.Mom;
                return FamilyMemberId.All;
            }

            if (scope.Contains("全家"))
                return FamilyMemberId.All;
            if (scope.Contains("父母"))
                return FamilyMemberId.Parents;
            if (scope.Contains("儿女") || scope.Contains("兒女"))
                return FamilyMemberId.Children;
            if (scope.Contains("兄妹") || scope.Contains("姐弟"))
                return FamilyMemberId.Brother | FamilyMemberId.Sister;
            if (scope.Contains("父子"))
                return FamilyMemberId.Dad | FamilyMemberId.Brother;
            if (scope.Contains("母女"))
                return FamilyMemberId.Mom | FamilyMemberId.Sister;
            if (scope.Contains("母子"))
                return FamilyMemberId.Mom | FamilyMemberId.Brother;
            if (scope.Contains("爸爸") || scope.Contains("爸"))
                return FamilyMemberId.Dad;
            if (scope.Contains("妈妈") || scope.Contains("媽媽") || scope.Contains("妈") || scope.Contains("媽"))
                return FamilyMemberId.Mom;
            if (scope.Contains("哥哥") || scope.Contains("哥"))
                return FamilyMemberId.Brother;
            if (scope.Contains("妹妹") || scope.Contains("妹"))
                return FamilyMemberId.Sister;
            if (scope.Contains("触发") || scope.Contains("觸發") || scope.Contains("操作") || scope.Contains("协助") || scope.Contains("協助"))
                return FamilyMemberId.None;

            return FamilyMemberId.All;
        }
    }
}
