using TMPro;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

namespace Platformer.UI
{
    public static class UIFontProvider
    {
        const string SourceFontPath = "Assets/TextMesh Pro/Fonts/jf-openhuninn-2.1.ttf";
        const string FontAssetPath = "Assets/TextMesh Pro/Fonts/jf-openhuninn-2.1 SDF.asset";

        static TMP_FontAsset primary;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            primary = null;
        }

        public static TMP_FontAsset Primary
        {
            get
            {
                if (primary != null)
                    return primary;

                Font sourceFont = null;
#if UNITY_EDITOR
                sourceFont = UnityEditor.AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
#endif
                var configuredFont = TMP_Settings.defaultFontAsset;
                if (sourceFont == null && configuredFont != null)
                    sourceFont = configuredFont.sourceFontFile;

                if (sourceFont != null)
                {
                    primary = TMP_FontAsset.CreateFontAsset(
                        sourceFont,
                        90,
                        9,
                        GlyphRenderMode.SDFAA,
                        1024,
                        1024,
                        AtlasPopulationMode.Dynamic,
                        true);
                    primary.name = "Runtime Traditional Chinese Font";
                    primary.hideFlags = HideFlags.DontSave;
                }
                else
                {
#if UNITY_EDITOR
                    primary = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
#endif
                    if (primary == null)
                        primary = configuredFont;
                    Debug.LogError("找不到繁體中文字型來源，無法建立動態 TMP 字型。");
                }

                return primary;
            }
        }

        public static void Apply(TMP_Text text)
        {
            if (text == null)
                return;

            var font = Primary;
            if (font == null)
                return;

            text.font = font;
        }

        public static void ApplyToHierarchy(Transform root)
        {
            if (root == null)
                return;

            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
                Apply(text);
        }
    }
}
