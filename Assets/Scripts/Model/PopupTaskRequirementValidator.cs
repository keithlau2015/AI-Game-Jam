using System.Collections.Generic;
using Platformer.Mechanics;
using Platformer.Model;

namespace Platformer.Model
{
    public static class PopupTaskRequirementValidator
    {
        public static bool MeetsRequirement(PopupTaskRequirement requirement, IReadOnlyList<WorkerUnit> assigned)
        {
            if (requirement == null)
                return assigned != null && assigned.Count > 0;

            if (assigned == null)
                return false;

            if (requirement.requireAllFamily)
                return assigned.Count == 4 && CoversAllFamily(assigned);

            if (assigned.Count < requirement.minParticipants || assigned.Count > requirement.maxParticipants)
                return false;

            for (var i = 0; i < assigned.Count; i++)
            {
                if ((requirement.forbiddenMask & assigned[i].familyMember) != 0)
                    return false;
            }

            if (requirement.requiredGroups == null || requirement.requiredGroups.Length == 0)
                return assigned.Count >= requirement.minParticipants;

            for (var g = 0; g < requirement.requiredGroups.Length; g++)
            {
                var group = requirement.requiredGroups[g];
                if (group == FamilyMemberId.All)
                    continue;

                var matched = false;
                for (var i = 0; i < assigned.Count; i++)
                {
                    if ((group & assigned[i].familyMember) != 0)
                    {
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                    return false;
            }

            return true;
        }

        public static bool CanAcceptMember(PopupTaskRequirement requirement, WorkerUnit worker, IReadOnlyList<WorkerUnit> assigned)
        {
            if (worker == null || requirement == null)
                return false;

            if ((requirement.forbiddenMask & worker.familyMember) != 0)
                return false;

            if (assigned != null && assigned.Count >= requirement.maxParticipants)
                return false;

            if (requirement.requiredGroups == null || requirement.requiredGroups.Length == 0)
                return true;

            for (var g = 0; g < requirement.requiredGroups.Length; g++)
            {
                var group = requirement.requiredGroups[g];
                if (group == FamilyMemberId.All)
                    return true;
                if ((group & worker.familyMember) != 0)
                    return true;
            }

            return requirement.requiredGroups.Length == 0;
        }

        static bool CoversAllFamily(IReadOnlyList<WorkerUnit> assigned)
        {
            var mask = FamilyMemberId.None;
            for (var i = 0; i < assigned.Count; i++)
                mask |= assigned[i].familyMember;
            return mask == FamilyMemberId.All;
        }

        public static string BuildRequirementLabel(PopupTaskRequirement requirement, string raw)
        {
            if (!string.IsNullOrEmpty(raw))
                return raw;
            if (requirement == null)
                return "Anyone";
            if (requirement.requireAllFamily)
                return "Whole family required";
            return $"{requirement.minParticipants}-{requirement.maxParticipants} members";
        }
    }
}
