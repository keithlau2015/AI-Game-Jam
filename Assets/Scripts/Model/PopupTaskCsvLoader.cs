using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Platformer.Model
{
    public static class PopupTaskCsvLoader
    {
        public static List<PopupTaskDefinition> Load(TextAsset csvAsset)
        {
            var results = new List<PopupTaskDefinition>();
            if (csvAsset == null || string.IsNullOrWhiteSpace(csvAsset.text))
                return results;

            var rows = ParseRows(csvAsset.text);
            if (rows.Count <= 1)
                return results;

            for (var i = 1; i < rows.Count; i++)
            {
                var columns = rows[i];
                if (columns.Count < 10)
                    continue;

                var title = columns[0].Trim();
                if (string.IsNullOrEmpty(title))
                    continue;

                var icon = columns[1].Trim();
                var maxParticipants = ParseInt(columns[2], 1);
                var roleRequirement = columns[3].Trim();
                var existenceDuration = ParseFloat(columns[4], 15f);
                var spawnMin = ParseFloat(columns[5], 10f);
                var spawnMax = ParseFloat(columns[6], 60f);
                var workDuration = ParseFloat(columns[7], 8f);
                var successRaw = columns[8].Trim();
                var failureRaw = columns[9].Trim();

                var requirement = PopupTaskRequirementParser.Parse(roleRequirement, maxParticipants);
                var definition = new PopupTaskDefinition
                {
                    taskId = $"popup_{i}_{SanitizeId(title)}",
                    title = title,
                    icon = icon,
                    maxParticipants = maxParticipants,
                    roleRequirementRaw = roleRequirement,
                    requirement = requirement,
                    existenceDuration = existenceDuration,
                    spawnTimeMin = spawnMin,
                    spawnTimeMax = spawnMax,
                    workDuration = workDuration,
                    successOutcomeRaw = successRaw,
                    failureOutcomeRaw = failureRaw,
                    successEffects = PopupTaskOutcomeParser.Parse(successRaw),
                    failureEffects = PopupTaskOutcomeParser.Parse(failureRaw)
                };
                results.Add(definition);
            }

            return results;
        }

        static List<List<string>> ParseRows(string text)
        {
            var rows = new List<List<string>>();
            var currentRow = new List<string>();
            var currentField = new System.Text.StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < text.Length; i++)
            {
                var c = text[i];
                if (c == '"')
                {
                    if (inQuotes && i + 1 < text.Length && text[i + 1] == '"')
                    {
                        currentField.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                    continue;
                }

                if (!inQuotes && (c == '\t' || c == ','))
                {
                    currentRow.Add(currentField.ToString());
                    currentField.Clear();
                    continue;
                }

                if (!inQuotes && (c == '\n' || c == '\r'))
                {
                    if (c == '\r' && i + 1 < text.Length && text[i + 1] == '\n')
                        i++;
                    currentRow.Add(currentField.ToString());
                    currentField.Clear();
                    if (currentRow.Count > 0)
                        rows.Add(currentRow);
                    currentRow = new List<string>();
                    continue;
                }

                currentField.Append(c);
            }

            if (currentField.Length > 0 || currentRow.Count > 0)
            {
                currentRow.Add(currentField.ToString());
                rows.Add(currentRow);
            }

            return rows;
        }

        static int ParseInt(string raw, int fallback)
        {
            return int.TryParse(raw?.Trim(), out var value) ? value : fallback;
        }

        static float ParseFloat(string raw, float fallback)
        {
            return float.TryParse(raw?.Trim(), out var value) ? value : fallback;
        }

        static string SanitizeId(string title)
        {
            var cleaned = Regex.Replace(title, @"[^\w]+", "_");
            return cleaned.Trim('_');
        }
    }
}
