using System;

namespace Platformer.Model
{
    [Flags]
    public enum WorkerColor
    {
        None = 0,
        Orange = 1,
        Blue = 2,
        Green = 4,
        All = Orange | Blue | Green
    }

    public static class WorkerColorRules
    {
        public static WorkerColor FromRole(WorkerRole role)
        {
            return role switch
            {
                WorkerRole.Builder => WorkerColor.Orange,
                WorkerRole.Analyst => WorkerColor.Blue,
                WorkerRole.Courier => WorkerColor.Green,
                _ => WorkerColor.All
            };
        }

        public static string GetDisplayName(WorkerColor color)
        {
            return color switch
            {
                WorkerColor.Orange => "Orange",
                WorkerColor.Blue => "Blue",
                WorkerColor.Green => "Green",
                _ => string.Empty
            };
        }

        public static string BuildAllowedColorsLabel(bool acceptAnyMember, WorkerColor allowedMemberColors)
        {
            if (acceptAnyMember)
                return "Any member";

            if (allowedMemberColors == WorkerColor.None)
                return "No members";

            if (allowedMemberColors == WorkerColor.All)
                return "Any member";

            var parts = new System.Collections.Generic.List<string>();
            if ((allowedMemberColors & WorkerColor.Orange) != 0)
                parts.Add("Orange");
            if ((allowedMemberColors & WorkerColor.Blue) != 0)
                parts.Add("Blue");
            if ((allowedMemberColors & WorkerColor.Green) != 0)
                parts.Add("Green");

            return parts.Count == 0 ? "No members" : string.Join(", ", parts);
        }
    }
}
