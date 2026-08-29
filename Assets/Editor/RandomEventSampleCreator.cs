using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Platformer.Model;
using UnityEditor;
using UnityEngine;

namespace Platformer.Editor
{
    public static class FamilyRandomEventImporter
    {
        const string Root = "Assets/Data/RandomEvents";
        const string CsvPath = Root + "/family_events.csv";
        const string PoolPath = Root + "/WorkerRandomEventPool.asset";
        const float EffectMagnitude = 10f;

        static readonly string[] LegacyEventPaths =
        {
            Root + "/Event_WorkerStrike.asset",
            Root + "/Event_SurpriseAudit.asset",
            Root + "/Event_OfficeRumor.asset"
        };

        [MenuItem("Platformer/Import Traditional Chinese Family Events")]
        public static void Import()
        {
            var csvAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(CsvPath);
            if (csvAsset == null)
                throw new FileNotFoundException($"找不到事件資料：{CsvPath}");

            var rows = ParseCsv(csvAsset.text);
            ValidateHeader(rows);

            var events = new List<RandomEventDefinition>();
            for (var rowIndex = 1; rowIndex < rows.Count; rowIndex++)
            {
                var row = rows[rowIndex];
                if (row.Count == 1 && string.IsNullOrWhiteSpace(row[0]))
                    continue;
                if (row.Count != 12)
                    throw new InvalidDataException($"CSV 第 {rowIndex + 1} 行應有 12 欄，實際為 {row.Count} 欄。");

                var fileName = $"Event_Family_{rowIndex:00}";
                var definition = LoadOrCreateEvent($"{Root}/{fileName}.asset");
                definition.title = row[0];
                definition.viewpointCharacter = row[1];
                definition.description = row[2];
                definition.weight = 1;
                definition.canRepeat = false;
                definition.options = new[]
                {
                    CreateOption(row[3], row[4], row[5]),
                    CreateOption(row[6], row[7], row[8]),
                    CreateOption(row[9], row[10], row[11])
                };
                EditorUtility.SetDirty(definition);
                events.Add(definition);
            }

            foreach (var legacyPath in LegacyEventPaths)
                AssetDatabase.DeleteAsset(legacyPath);

            var pool = AssetDatabase.LoadAssetAtPath<RandomEventPool>(PoolPath);
            if (pool == null)
            {
                pool = ScriptableObject.CreateInstance<RandomEventPool>();
                AssetDatabase.CreateAsset(pool, PoolPath);
            }

            pool.events = events.ToArray();
            pool.triggerOnTimer = true;
            pool.intervalMin = 18f;
            pool.intervalMax = 32f;
            pool.initialDelay = 12f;
            EditorUtility.SetDirty(pool);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = pool;
            EditorGUIUtility.PingObject(pool);
            Debug.Log($"已匯入 {events.Count} 個繁體中文家庭隨機事件。");
        }

        static RandomEventDefinition LoadOrCreateEvent(string path)
        {
            var definition = AssetDatabase.LoadAssetAtPath<RandomEventDefinition>(path);
            if (definition != null)
                return definition;

            definition = ScriptableObject.CreateInstance<RandomEventDefinition>();
            AssetDatabase.CreateAsset(definition, path);
            return definition;
        }

        static RandomEventOption CreateOption(string label, string effectText, string outcomeText)
        {
            var effects = ParseEffects(effectText);
            var option = new RandomEventOption
            {
                label = label,
                outcomeText = outcomeText
            };

            if (effects.Count > 0)
            {
                option.effectType = effects[0].type;
                option.value = effects[0].value;
            }
            if (effects.Count > 1)
            {
                option.secondaryEffectType = effects[1].type;
                option.secondaryValue = effects[1].value;
            }
            if (effects.Count > 2)
            {
                option.tertiaryEffectType = effects[2].type;
                option.tertiaryValue = effects[2].value;
            }

            return option;
        }

        static List<EffectValue> ParseEffects(string effectText)
        {
            var effects = new List<EffectValue>();
            var tokens = effectText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var token in tokens)
            {
                if (token.EndsWith("0", StringComparison.Ordinal))
                    continue;

                var value = token.EndsWith("+", StringComparison.Ordinal)
                    ? EffectMagnitude
                    : token.EndsWith("-", StringComparison.Ordinal)
                        ? -EffectMagnitude
                        : 0f;
                if (Mathf.Approximately(value, 0f))
                    continue;

                if (token.StartsWith("希望", StringComparison.Ordinal))
                    effects.Add(new EffectValue(RandomEventEffectType.AddHope, value));
                else if (token.StartsWith("和諧", StringComparison.Ordinal))
                    effects.Add(new EffectValue(RandomEventEffectType.ModifyRapport, value));
                else if (token.StartsWith("情緒", StringComparison.Ordinal))
                    effects.Add(new EffectValue(RandomEventEffectType.ModifyStress, value));
                else
                    throw new InvalidDataException($"未知的事件效果：{token}");
            }

            return effects;
        }

        static void ValidateHeader(IReadOnlyList<List<string>> rows)
        {
            if (rows.Count == 0 || rows[0].Count != 12
                || rows[0][0] != "標題"
                || rows[0][1] != "視點主角"
                || rows[0][11] != "選擇3故事內文")
            {
                throw new InvalidDataException("家庭事件 CSV 標題列格式不正確。");
            }
        }

        static List<List<string>> ParseCsv(string csv)
        {
            var rows = new List<List<string>>();
            var row = new List<string>();
            var field = new StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < csv.Length; i++)
            {
                var character = csv[i];
                if (character == '"')
                {
                    if (inQuotes && i + 1 < csv.Length && csv[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }
                }
                else if (character == ',' && !inQuotes)
                {
                    row.Add(field.ToString());
                    field.Clear();
                }
                else if ((character == '\n' || character == '\r') && !inQuotes)
                {
                    if (character == '\r' && i + 1 < csv.Length && csv[i + 1] == '\n')
                        i++;
                    row.Add(field.ToString());
                    field.Clear();
                    rows.Add(row);
                    row = new List<string>();
                }
                else
                {
                    field.Append(character);
                }
            }

            if (field.Length > 0 || row.Count > 0)
            {
                row.Add(field.ToString());
                rows.Add(row);
            }

            return rows;
        }

        readonly struct EffectValue
        {
            public readonly RandomEventEffectType type;
            public readonly float value;

            public EffectValue(RandomEventEffectType type, float value)
            {
                this.type = type;
                this.value = value;
            }
        }
    }
}
