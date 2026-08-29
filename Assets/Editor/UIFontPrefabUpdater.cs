using Platformer.UI;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace Platformer.Editor
{
    public static class UIFontPrefabUpdater
    {
        const string FontPath = "Assets/TextMesh Pro/Fonts/jf-openhuninn-2.1 SDF.asset";

        [MenuItem("Platformer/Fix UI Fonts")]
        public static void FixUIFonts()
        {
            var font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontPath);
            if (font == null)
            {
                Debug.LogError("Missing UI font at " + FontPath);
                return;
            }

            var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
            var updated = 0;

            foreach (var guid in prefabGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.StartsWith("Assets/"))
                    continue;

                if (UpdatePrefabFonts(path, font))
                    updated++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Updated fonts in " + updated + " prefabs.");
        }

        static bool UpdatePrefabFonts(string path, TMP_FontAsset font)
        {
            var root = PrefabUtility.LoadPrefabContents(path);
            var texts = root.GetComponentsInChildren<TMP_Text>(true);
            var changed = false;

            foreach (var text in texts)
            {
                if (text.font == font)
                    continue;

                text.font = font;
                changed = true;
            }

            if (!changed)
            {
                PrefabUtility.UnloadPrefabContents(root);
                return false;
            }

            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
            return true;
        }
    }
}
