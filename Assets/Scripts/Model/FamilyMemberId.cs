using System;
using System.Collections.Generic;

namespace Platformer.Model
{
    [Flags]
    public enum FamilyMemberId
    {
        None = 0,
        Dad = 1,
        Mom = 2,
        Brother = 4,
        Sister = 8,
        Parents = Dad | Mom,
        Children = Brother | Sister,
        All = Dad | Mom | Brother | Sister
    }

    public static class FamilyMemberRules
    {
        public static FamilyMemberId FromDisplayName(string displayName)
        {
            if (string.IsNullOrEmpty(displayName))
                return FamilyMemberId.None;

            switch (displayName.Trim())
            {
                case "Dad":
                    return FamilyMemberId.Dad;
                case "Mom":
                    return FamilyMemberId.Mom;
                case "Leo":
                    return FamilyMemberId.Brother;
                case "Mia":
                    return FamilyMemberId.Sister;
                default:
                    return FamilyMemberId.None;
            }
        }

        public static string GetLabel(FamilyMemberId member)
        {
            if (member == FamilyMemberId.All)
                return "Whole family";

            var parts = new List<string>();
            if ((member & FamilyMemberId.Dad) != 0)
                parts.Add("Dad");
            if ((member & FamilyMemberId.Mom) != 0)
                parts.Add("Mom");
            if ((member & FamilyMemberId.Brother) != 0)
                parts.Add("Leo");
            if ((member & FamilyMemberId.Sister) != 0)
                parts.Add("Mia");
            return parts.Count == 0 ? "Anyone" : string.Join(", ", parts);
        }

        public static int CountMembers(FamilyMemberId mask)
        {
            var count = 0;
            foreach (FamilyMemberId value in Enum.GetValues(typeof(FamilyMemberId)))
            {
                if (value == FamilyMemberId.None || value == FamilyMemberId.All
                    || value == FamilyMemberId.Parents || value == FamilyMemberId.Children)
                    continue;
                if ((mask & value) != 0)
                    count++;
            }
            return count;
        }
    }
}
