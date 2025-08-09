using UnityEngine;
using UnityEngine.Serialization;

namespace Models
{
    /// <summary>
    /// Represents a fruit (nut) that can be bought in the store. Each fruit
    /// affects the Chao's mood, belly, and ability stats as described in
    /// the original Tiny Chao Garden games【908307047627800†L219-L236】.
    /// Positive numbers increase stats; negative numbers decrease them.
    /// </summary>
    [CreateAssetMenu(fileName = "FruitData", menuName = "TinyChaoGarden/Fruit", order = 1)]
    public class FruitData : ItemData
    {
        [FormerlySerializedAs("MoodEffect")] [Header("Stat Effects")]
        public int moodEffect;
        [FormerlySerializedAs("BellyEffect")] public int bellyEffect;
        [FormerlySerializedAs("SwimEffect")] public int swimEffect;
        [FormerlySerializedAs("FlyEffect")] public int flyEffect;
        [FormerlySerializedAs("RunEffect")] public int runEffect;
        [FormerlySerializedAs("PowerEffect")] public int powerEffect;
        [FormerlySerializedAs("StaminaEffect")] public int staminaEffect;
    }
}