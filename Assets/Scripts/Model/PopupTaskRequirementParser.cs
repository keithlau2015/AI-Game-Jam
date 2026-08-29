using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Platformer.Model
{
    public static class PopupTaskRequirementParser
    {
        public static PopupTaskRequirement Parse(string raw, int maxParticipants)
        {
            var requirement = new PopupTaskRequirement
            {
                maxParticipants = Mathf.Max(maxParticipants, 1),
                minParticipants = 1
            };

            if (string.IsNullOrWhiteSpace(raw))
            {
                requirement.minParticipants = 1;
                return requirement;
            }

            var text = raw.Trim();

            if (text.Contains("全家") && text.Contains("全部"))
            {
                requirement.requireAllFamily = true;
                requirement.minParticipants = 4;
                requirement.maxParticipants = 4;
                return requirement;
            }

            if (text.Contains("除了妈妈") || text.Contains("除了媽媽"))
            {
                requirement.forbiddenMask = FamilyMemberId.Mom;
                requirement.minParticipants = ExtractCount(text, 3);
                requirement.maxParticipants = requirement.minParticipants;
                requirement.requiredGroups = new[]
                {
                    FamilyMemberId.Dad | FamilyMemberId.Brother | FamilyMemberId.Sister
                };
                return requirement;
            }

            if (text.Contains("无限制") || text.Contains("無限制") || text.Contains("任意"))
            {
                requirement.minParticipants = ExtractCount(text, maxParticipants > 0 ? maxParticipants : 1);
                requirement.maxParticipants = requirement.minParticipants;
                requirement.requiredGroups = new[] { FamilyMemberId.All };
                return requirement;
            }

            requirement.requiredGroups = ParseRequiredGroups(text);
            if (requirement.requiredGroups.Length > 0)
            {
                var requiredCount = 0;
                for (var i = 0; i < requirement.requiredGroups.Length; i++)
                    requiredCount += FamilyMemberRules.CountMembers(requirement.requiredGroups[i]);
                requirement.minParticipants = Mathf.Max(requiredCount, 1);
            }

            requirement.maxParticipants = Mathf.Max(maxParticipants, requirement.minParticipants);
            return requirement;
        }

        static FamilyMemberId[] ParseRequiredGroups(string text)
        {
            var groups = new List<FamilyMemberId>();
            var segments = Regex.Split(text, @"\s+且\s+|\s+并且\s+");
            for (var i = 0; i < segments.Length; i++)
            {
                var segment = segments[i].Trim();
                if (segment.Length == 0)
                    continue;
                groups.Add(ParseSegment(segment));
            }

            if (groups.Count == 0)
                groups.Add(ParseSegment(text));
            return groups.ToArray();
        }

        static FamilyMemberId ParseSegment(string segment)
        {
            segment = segment.Replace("必须参加", string.Empty)
                .Replace("必須參加", string.Empty)
                .Replace("(", string.Empty)
                .Replace(")", string.Empty)
                .Replace("（", string.Empty)
                .Replace("）", string.Empty)
                .Trim();

            if (segment.Contains("哥或妹") || segment.Contains("兄或妹") || segment.Contains("儿女") || segment.Contains("兒女"))
                return FamilyMemberId.Brother | FamilyMemberId.Sister;

            if (segment.Contains("爸或妈") || segment.Contains("爸或媽") || segment.Contains("父母"))
                return FamilyMemberId.Dad | FamilyMemberId.Mom;

            var mask = FamilyMemberId.None;
            if (segment.Contains("爸爸") || segment.Contains("爸"))
                mask |= FamilyMemberId.Dad;
            if (segment.Contains("妈妈") || segment.Contains("媽媽") || segment.Contains("妈") || segment.Contains("媽"))
                mask |= FamilyMemberId.Mom;
            if (segment.Contains("哥哥") || segment.Contains("哥"))
                mask |= FamilyMemberId.Brother;
            if (segment.Contains("妹妹") || segment.Contains("妹"))
                mask |= FamilyMemberId.Sister;

            if (segment.Contains(" 或 ") || segment.Contains("或"))
            {
                if (mask == FamilyMemberId.None)
                    return FamilyMemberId.All;
                return mask;
            }

            return mask == FamilyMemberId.None ? FamilyMemberId.All : mask;
        }

        static int ExtractCount(string text, int fallback)
        {
            var match = Regex.Match(text, @"(\d+)");
            if (!match.Success)
                return fallback;
            return int.Parse(match.Groups[1].Value);
        }
    }
}
