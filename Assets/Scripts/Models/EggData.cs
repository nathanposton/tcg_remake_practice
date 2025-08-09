using UnityEngine;
using UnityEngine.Serialization;

namespace Models
{
    /// <summary>
    /// Represents a Chao egg available for purchase in the store. Eggs come
    /// in different colours (Normal, Jewel types) and have a rarity. Only
    /// one egg can be present in the garden at any given time【908307047627800†L242-L246】.
    /// </summary>
    [CreateAssetMenu(fileName = "EggData", menuName = "TinyChaoGarden/Egg", order = 2)]
    public class EggData : ItemData
    {
        [FormerlySerializedAs("EggSprite")] [Tooltip("Sprite used to represent the egg in the store and garden.")]
        public Sprite eggSprite;

        [FormerlySerializedAs("Rarity")]
        [Tooltip("Indicates how rare this egg is when the store randomises its stock (0-1). Higher is rarer).")]
        [Range(0f, 1f)]
        public float rarity;

        [FormerlySerializedAs("IsJewel")] [Tooltip("Whether the hatched Chao will have a jewel (metallic) coat.")]
        public bool isJewel;
    }
}