using UnityEditor;
using UnityEngine;

namespace Platformer.EditorTools
{
    public static class FamilySpriteImportFixer
    {
        [MenuItem("Platformer/Fix Family Sprite Imports")]
        public static void FixAll()
        {
            var paths = new[]
            {
                "Assets/Resources/FamilySprites/father-base-sprite-64x96.png",
                "Assets/Resources/FamilySprites/mother-base-sprite-64x96.png",
                "Assets/Resources/FamilySprites/older-daughter-base-sprite-64x96.png",
                "Assets/Resources/FamilySprites/younger-son-base-sprite-64x96.png"
            };

            foreach (var path in paths)
                Apply(path);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static void Apply(string path)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                return;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 64f;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
    }
}
