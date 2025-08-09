using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Models
{
    /// <summary>
    /// Enumeration representing the stage or type of a Chao. In the Tiny Chao
    /// Garden the Chao never ages, but its stage is synced with the main
    /// Chao Garden if transferred.
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
    /// negative effects from fruits do not decrease the level below 0.
    /// </summary>
    [Serializable]
    public class ChaoStats
    {
        [FormerlySerializedAs("Name")] public string name;
        [FormerlySerializedAs("Stage")] public ChaoStage stage = ChaoStage.Child;

        // Backing fields with serialization preserved. Properties enforce clamping.
        [SerializeField, Range(0f, 1f)] private float _mood = 1f;
        [SerializeField, Range(0f, 1f)] private float _belly = 1f;

        [SerializeField, FormerlySerializedAs("Swim"), FormerlySerializedAs("swim"), Range(0, 99)] private int _swim;
        [SerializeField, FormerlySerializedAs("Fly"), FormerlySerializedAs("fly"), Range(0, 99)] private int _fly;
        [SerializeField, FormerlySerializedAs("Run"), FormerlySerializedAs("run"), Range(0, 99)] private int _run;
        [SerializeField, FormerlySerializedAs("Power"), FormerlySerializedAs("power"), Range(0, 99)] private int _power;
        [SerializeField, FormerlySerializedAs("Stamina"), FormerlySerializedAs("stamina"), Range(0, 99)] private int _stamina;

        [FormerlySerializedAs("Rings")] public int rings;

        public float mood
        {
            get => _mood;
            set => _mood = Mathf.Clamp01(value);
        }

        public float belly
        {
            get => _belly;
            set => _belly = Mathf.Clamp01(value);
        }

        public int swim
        {
            get => _swim;
            set => _swim = ClampStat(value);
        }

        public int fly
        {
            get => _fly;
            set => _fly = ClampStat(value);
        }

        public int run
        {
            get => _run;
            set => _run = ClampStat(value);
        }

        public int power
        {
            get => _power;
            set => _power = ClampStat(value);
        }

        public int stamina
        {
            get => _stamina;
            set => _stamina = ClampStat(value);
        }

        private static int ClampStat(int value) => Mathf.Clamp(value, 0, 99);

        /// <summary>
        /// Apply the effects of a fruit to this Chao's stats. Stats are clamped
        /// within appropriate bounds and cannot fall below zero or exceed 1 for
        /// Mood/Belly. Once a level threshold is reached the level never
        /// decreases.
        /// </summary>
        public void ApplyFruit(FruitData fruit)
        {
            if (fruit == null) return;
            mood = Mathf.Clamp01(mood + fruit.moodEffect / 5f);
            belly = Mathf.Clamp01(belly + fruit.bellyEffect / 5f);

            if (fruit.swimEffect > 0) swim = ClampStat(swim + fruit.swimEffect);
            if (fruit.flyEffect > 0) fly = ClampStat(fly + fruit.flyEffect);
            if (fruit.runEffect > 0) run = ClampStat(run + fruit.runEffect);
            if (fruit.powerEffect > 0) power = ClampStat(power + fruit.powerEffect);
            if (fruit.staminaEffect > 0) stamina = ClampStat(stamina + fruit.staminaEffect);
        }
    }
}