using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Models
{
    /// <summary>
    /// Enumeration representing the stage or type of a Chao. In the Tiny Chao
    /// Garden the Chao never ages, but its stage is synced with the main
    /// Chao Garden if transferred【821806795223146†L530-L545】.
    /// </summary>
    public enum ChaoStage
    {
        Child,
        Normal,
        Swim,
        Fly,
        Run,
        Power,
        Chaos
    }

    /// <summary>
    /// Represents the statistics of a Chao. Values are kept simple but can be
    /// extended with additional state as needed. Mood and belly are floats
    /// between 0 and 1 representing how happy and full the Chao is. Ability
    /// stats range from 0–99 as in the original game and only increase;
    /// negative effects from fruits do not decrease the level below 0【908307047627800†L171-L195】.
    /// </summary>
    [Serializable]
    public class ChaoStats
    {
        [FormerlySerializedAs("Name")] public string name;
        [FormerlySerializedAs("Stage")] public ChaoStage stage = ChaoStage.Child;
        [FormerlySerializedAs("Mood")] [Range(0f, 1f)]
        public float mood = 1f;
        [FormerlySerializedAs("Belly")] [Range(0f, 1f)]
        public float belly = 1f;
        [FormerlySerializedAs("Swim")] [Range(0, 99)]
        public int swim;
        [FormerlySerializedAs("Fly")] [Range(0, 99)]
        public int fly;
        [FormerlySerializedAs("Run")] [Range(0, 99)]
        public int run;
        [FormerlySerializedAs("Power")] [Range(0, 99)]
        public int power;
        [FormerlySerializedAs("Stamina")] [Range(0, 99)]
        public int stamina;
        [FormerlySerializedAs("Rings")] public int rings;

        /// <summary>
        /// Apply the effects of a fruit to this Chao's stats. Stats are clamped
        /// within appropriate bounds and cannot fall below zero or exceed 1 for
        /// Mood/Belly. Once a level threshold is reached the level never
        /// decreases【908307047627800†L171-L195】.
        /// </summary>
        public void ApplyFruit(FruitData fruit)
        {
            if (fruit == null) return;
            mood = Mathf.Clamp01(mood + fruit.moodEffect / 5f); // divide by 5 to map bars to 0–1
            belly = Mathf.Clamp01(belly + fruit.bellyEffect / 5f);
            // Ability stats increase by one level if cumulative effect reaches threshold.
            ApplyStatEffect(ref swim, fruit.swimEffect);
            ApplyStatEffect(ref fly, fruit.flyEffect);
            ApplyStatEffect(ref run, fruit.runEffect);
            ApplyStatEffect(ref power, fruit.powerEffect);
            ApplyStatEffect(ref stamina, fruit.staminaEffect);
        }

        private void ApplyStatEffect(ref int stat, int effect)
        {
            if (effect <= 0) return;
            stat = Mathf.Clamp(stat + effect, 0, 99);
        }
    }
}