using UnityEngine;

namespace Platformer.Model
{
    public static class FamilyMemberSpriteRules
    {
        static Sprite dadSprite;
        static Sprite momSprite;
        static Sprite sisterSprite;
        static Sprite brotherSprite;

        public static Sprite GetSprite(FamilyMemberId member)
        {
            switch (member)
            {
                case FamilyMemberId.Dad:
                    return Load(ref dadSprite, "FamilySprites/father-base-sprite-64x96");
                case FamilyMemberId.Mom:
                    return Load(ref momSprite, "FamilySprites/mother-base-sprite-64x96");
                case FamilyMemberId.Sister:
                    return Load(ref sisterSprite, "FamilySprites/older-daughter-base-sprite-64x96");
                case FamilyMemberId.Brother:
                    return Load(ref brotherSprite, "FamilySprites/younger-son-base-sprite-64x96");
                default:
                    return null;
            }
        }

        static Sprite Load(ref Sprite cache, string resourcePath)
        {
            if (cache != null)
                return cache;

            cache = Resources.Load<Sprite>(resourcePath);
            if (cache == null)
            {
                var sprites = Resources.LoadAll<Sprite>(resourcePath);
                if (sprites != null && sprites.Length > 0)
                    cache = sprites[0];
            }

            return cache;
        }
    }
}
