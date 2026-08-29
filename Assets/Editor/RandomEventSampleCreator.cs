using Platformer.Model;
using UnityEditor;
using UnityEngine;

namespace Platformer.Editor
{
    public static class RandomEventSampleCreator
    {
        const string Root = "Assets/Data/RandomEvents";

        [MenuItem("Platformer/Create Sample Random Events")]
        public static void CreateSamples()
        {
            EnsureFolder("Assets/Data");
            EnsureFolder(Root);

            var strike = CreateEvent("Event_WorkerStrike",
                "Morale crisis on the floor",
                "Workers stop and demand a decision that has nothing to do with output charts.",
                new[]
                {
                    Option("Offer a break", "Morale rises. You lose a little time.", RandomEventEffectType.ModifyMorale, 15, RandomEventEffectType.RemoveTime, 8),
                    Option("Push through", "Production spikes, but spirits drop.", RandomEventEffectType.ModifyProductionRate, 0.35f, RandomEventEffectType.ModifyMorale, -12),
                    Option("Call it team building", "Reputation improves. Nobody knows why.", RandomEventEffectType.ModifyReputation, 4, RandomEventEffectType.NarrativeOnly, 0)
                });

            var audit = CreateEvent("Event_SurpriseAudit",
                "Inspectors arrive",
                "A clipboard appears. This was not in the level design document.",
                new[]
                {
                    Option("Hide the mess", "A random station goes offline.", RandomEventEffectType.DisableRandomStation, 0),
                    Option("Show transparency", "Karma increases.", RandomEventEffectType.AddKarma, 5),
                    Option("Bribe with coffee", "You buy time with caffeine diplomacy.", RandomEventEffectType.AddTime, 12)
                });

            var rumor = CreateEvent("Event_OfficeRumor",
                "A rumor spreads",
                "Someone says the goalpost moved. It might be metaphorical. It might not.",
                new[]
                {
                    Option("Deny it", "Morale falls slightly.", RandomEventEffectType.ModifyMorale, -8),
                    Option("Lean into it", "Output slows, but workers feel heard.", RandomEventEffectType.ModifyProductionRate, -0.2f, RandomEventEffectType.ModifyMorale, 10),
                    Option("Change the subject", "Nothing happens. Probably.", RandomEventEffectType.NarrativeOnly, 0)
                });

            var pool = ScriptableObject.CreateInstance<RandomEventPool>();
            pool.events = new[] { strike, audit, rumor };
            pool.triggerOnTimer = true;
            pool.intervalMin = 18f;
            pool.intervalMax = 32f;
            pool.initialDelay = 12f;

            var poolPath = $"{Root}/WorkerRandomEventPool.asset";
            AssetDatabase.CreateAsset(pool, poolPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = pool;
            EditorGUIUtility.PingObject(pool);
        }

        static RandomEventDefinition CreateEvent(string fileName, string title, string description, RandomEventOption[] options)
        {
            var definition = ScriptableObject.CreateInstance<RandomEventDefinition>();
            definition.title = title;
            definition.description = description;
            definition.weight = 1;
            definition.canRepeat = true;
            definition.options = options;
            var path = $"{Root}/{fileName}.asset";
            AssetDatabase.CreateAsset(definition, path);
            return definition;
        }

        static RandomEventOption Option(string label, string outcome, RandomEventEffectType effect, float value, RandomEventEffectType secondEffect = RandomEventEffectType.None, float secondValue = 0f)
        {
            return new RandomEventOption
            {
                label = label,
                outcomeText = outcome,
                effectType = effect,
                value = value,
                secondaryEffectType = secondEffect,
                secondaryValue = secondValue
            };
        }

        static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parent = System.IO.Path.GetDirectoryName(path)?.Replace("\\", "/");
                var folderName = System.IO.Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folderName);
            }
        }
    }
}
