using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Platformer.Editor
{
    /// <summary>
    /// Generates walking + idle animations and animator controllers for each family member
    /// from the sprite sheets in Assets/Character/Sprites/Family/Source, and writes a
    /// runtime mapping (FamilyAnimationConfig) used by WorkerUnit to pick the right controller.
    /// </summary>
    public static class SetupFamilyAnimations
    {
        const string SourceDir = "Assets/Character/Sprites/Family/Source";
        const string OutputDir = "Assets/Character/Animations/Family";
        const float PixelsPerUnit = 64f;
        const float WalkFps = 8f;

        class FamilyDef
        {
            public string id;
            public string baseFile;
            public string movementFile;
            public List<string> workerNames;
        }

        static List<FamilyDef> Families()
        {
            return new List<FamilyDef>
            {
                new FamilyDef { id = "father", baseFile = "father-base.png", movementFile = "father-movement.png", workerNames = new List<string> { "Dad" } },
                new FamilyDef { id = "mother", baseFile = "mother-base.png", movementFile = "mother-movement.png", workerNames = new List<string> { "Mom" } },
                new FamilyDef { id = "daughter", baseFile = "daughter-base.png", movementFile = "daughter-movement.png", workerNames = new List<string> { "Mia" } },
                new FamilyDef { id = "son", baseFile = "son-base.png", movementFile = "son-movement.png", workerNames = new List<string> { "Leo" } },
                new FamilyDef { id = "corgi", baseFile = "corgi-base.png", movementFile = "corgi-movement.png", workerNames = new List<string> { "Corgi" } },
            };
        }

        [MenuItem("Tools/Setup Family Animations")]
        public static void Setup()
        {
            foreach (var fam in Families())
                BuildFamily(fam);
            BuildConfig();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SetupFamilyAnimations] Family animations generated.");
        }

        static void BuildFamily(FamilyDef fam)
        {
            var basePath = Path.Combine(SourceDir, fam.baseFile);
            var movePath = Path.Combine(SourceDir, fam.movementFile);

            ConfigureBaseSprite(basePath);
            var baseSprite = AssetDatabase.LoadAssetAtPath<Sprite>(basePath);

            ConfigureMovementSheet(movePath, out int rows, out int cols);
            var sprites = AssetDatabase.LoadAllAssetsAtPath(movePath);
            var walkSprites = new Dictionary<string, List<Sprite>>();
            foreach (var o in sprites)
            {
                if (o is Sprite s)
                {
                    var n = s.name;
                    if (n.StartsWith("forward_") || n.StartsWith("backward_") || n.StartsWith("left_") || n.StartsWith("right_"))
                    {
                        var key = n.Substring(0, n.IndexOf('_'));
                        if (!walkSprites.ContainsKey(key)) walkSprites[key] = new List<Sprite>();
                        walkSprites[key].Add(s);
                    }
                }
            }
            foreach (var kv in walkSprites) kv.Value.Sort((a, b) => int.Parse(a.name.Substring(a.name.IndexOf('_') + 1)));

            var dir = Path.Combine(OutputDir, fam.id);
            if (!AssetDatabase.IsValidFolder(dir)) AssetDatabase.CreateFolder(OutputDir, fam.id);

            var idleClip = BuildIdleClip(Path.Combine(dir, fam.id + "_Idle.anim"), baseSprite);
            var forward = BuildWalkClip(Path.Combine(dir, fam.id + "_WalkForward.anim"), GetRow(walkSprites, "forward"));
            var backward = BuildWalkClip(Path.Combine(dir, fam.id + "_WalkBackward.anim"), GetRow(walkSprites, "backward"));
            var left = BuildWalkClip(Path.Combine(dir, fam.id + "_WalkLeft.anim"), GetRow(walkSprites, "left"));
            var right = BuildWalkClip(Path.Combine(dir, fam.id + "_WalkRight.anim"), GetRow(walkSprites, "right"));

            BuildController(Path.Combine(dir, fam.id + "Animator.controller"), idleClip, forward, backward, left, right, baseSprite);
        }

        static List<Sprite> GetRow(Dictionary<string, List<Sprite>> map, string key)
        {
            return map.TryGetValue(key, out var list) ? list : new List<Sprite>();
        }

        static void ConfigureBaseSprite(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) return;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.spritePivot = new Vector2(0.5f, 0.5f);
            importer.SaveAndReimport();
        }

        static void ConfigureMovementSheet(string path, out int rows, out int cols)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit = PixelsPerUnit;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;

            var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            cols = tex.width / 64;
            rows = tex.height / 96;

            var metas = new List<SpriteMetaData>();
            string[] dirNames = { "forward", "backward", "left", "right" };
            for (int r = 0; r < rows; r++)
            {
                var dirName = r < dirNames.Length ? dirNames[r] : "row" + r;
                for (int c = 0; c < cols; c++)
                {
                    var meta = new SpriteMetaData
                    {
                        name = $"{dirName}_{c}",
                        rect = new Rect(c * 64f, (rows - 1 - r) * 96f, 64f, 96f),
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f),
                    };
                    metas.Add(meta);
                }
            }
            importer.sprites = metas.ToArray();
            importer.SaveAndReimport();
        }

        static AnimationClip BuildIdleClip(string path, Sprite idle)
        {
            var clip = new AnimationClip { frameRate = 1f, wrapMode = WrapMode.Loop };
            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            var keys = new ObjectReferenceKeyframe[1];
            keys[0] = new ObjectReferenceKeyframe { time = 0f, value = idle };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        static AnimationClip BuildWalkClip(string path, List<Sprite> frames)
        {
            var clip = new AnimationClip { frameRate = WalkFps, wrapMode = WrapMode.Loop };
            if (frames == null || frames.Count == 0)
            {
                Debug.LogWarning("[SetupFamilyAnimations] Missing walk frames for " + path);
                return clip;
            }
            var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
            var keys = new ObjectReferenceKeyframe[frames.Count];
            float step = 1f / WalkFps;
            for (int i = 0; i < frames.Count; i++)
                keys[i] = new ObjectReferenceKeyframe { time = i * step, value = frames[i] };
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            settings.stopTime = frames.Count * step;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        static void BuildController(string path, AnimationClip idle, AnimationClip forward, AnimationClip backward, AnimationClip left, AnimationClip right, Sprite idleSprite)
        {
            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Moving", AnimatorControllerParameterType.Bool);
            controller.AddParameter("DirX", AnimatorControllerParameterType.Float);
            controller.AddParameter("DirY", AnimatorControllerParameterType.Float);

            var root = controller.layers[0].stateMachine;

            var idleState = root.AddState("Idle");
            idleState.motion = idle;
            idleState.writeDefaultValues = false;

            var walkState = root.AddState("Walk");
            var tree = new BlendTree
            {
                name = "WalkTree",
                blendType = BlendTreeType.SimpleDirectional2D,
                blendParameter = "DirX",
                blendParameterY = "DirY",
                useAutomaticThresholds = false,
            };
            tree.AddChild(forward, new Vector2(0f, 1f));   // backward (up)
            tree.AddChild(backward, new Vector2(0f, -1f)); // forward (down / toward screen)
            tree.AddChild(left, new Vector2(-1f, 0f));
            tree.AddChild(right, new Vector2(1f, 0f));
            walkState.motion = tree;
            walkState.writeDefaultValues = false;

            var toWalk = idleState.AddTransition(walkState);
            toWalk.AddCondition(AnimatorConditionMode.If, 0, "Moving");
            toWalk.hasExitTime = false;
            toWalk.duration = 0f;

            var toIdle = walkState.AddTransition(idleState);
            toIdle.AddCondition(AnimatorConditionMode.IfNot, 0, "Moving");
            toIdle.hasExitTime = false;
            toIdle.duration = 0f;

            AssetDatabase.SaveAssets();
        }

        static void BuildConfig()
        {
            var configDir = "Assets/Resources";
            if (!AssetDatabase.IsValidFolder(configDir)) AssetDatabase.CreateFolder("Assets", "Resources");

            var config = ScriptableObject.CreateInstance<FamilyAnimationConfig>();
            foreach (var fam in Families())
            {
                var controllerPath = Path.Combine(OutputDir, fam.id, fam.id + "Animator.controller");
                var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(controllerPath);
                if (controller == null)
                {
                    Debug.LogWarning("[SetupFamilyAnimations] Missing controller: " + controllerPath);
                    continue;
                }
                foreach (var name in fam.workerNames)
                {
                    var entry = new FamilyAnimationConfig.Entry
                    {
                        memberName = name,
                        controller = controller,
                    };
                    config.entries.Add(entry);
                }
            }
            var configPath = Path.Combine(configDir, "FamilyAnimationConfig.asset");
            AssetDatabase.CreateAsset(config, configPath);
        }
    }
}
