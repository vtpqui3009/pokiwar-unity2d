using UnityEngine;

namespace Pokiwar.Evolution
{
    /// <summary>
    /// ScriptableObject database mapping levels to pet sprites.
    /// </summary>
    [CreateAssetMenu(fileName = "SpriteDatabase", menuName = "Pokiwar/Sprite Database")]
    public class SpriteDatabase : ScriptableObject
    {
        [SerializeField] private Sprite[] petSprites;

        public int MaxSprites => petSprites?.Length ?? 0;

        public Sprite GetSpriteForLevel(int level)
        {
            if (petSprites == null || petSprites.Length == 0)
                return null;

            int index = Mathf.Clamp(level, 1, petSprites.Length) - 1;
            return petSprites[index];
        }

        public bool HasSprite(int level)
        {
            if (petSprites == null || petSprites.Length == 0)
                return false;
            return level >= 1 && level <= petSprites.Length;
        }
    }
}
