using TMPro;
using UnityEngine;

namespace Platformer.UI
{
    public static class UIFontProvider
    {
        const string FontAssetPath = "Assets/TextMesh Pro/Fonts/jf-openhuninn-2.1 SDF.asset";

        static TMP_FontAsset primary;

        public static TMP_FontAsset Primary
        {
            get
            {
                if (primary != null)
                    return primary;

#if UNITY_EDITOR
                primary = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(FontAssetPath);
#endif
                if (primary == null && TMP_Settings.defaultFontAsset != null)
                    primary = TMP_Settings.defaultFontAsset;

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
