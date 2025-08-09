using System;
using Managers;
using Models;
using UnityEngine;
using UnityEngine.Serialization;

namespace Components
{
    /// <summary>
    /// MonoBehaviour controlling a Chao instance in the garden. It holds the
    /// Chao's stats and updates them over time, allowing the player to pet
    /// or feed the Chao. It also handles the run‑away behaviour if both Mood
    /// and Belly remain empty for a prolonged period【908307047627800†L171-L189】.
    /// Attach this to the Chao prefab in the garden scene.
    /// </summary>
    public class ChaoController : MonoBehaviour
    {
        [FormerlySerializedAs("Stats")] public ChaoStats stats;

        [FormerlySerializedAs("MoodDecayRate")] [Tooltip("Rate at which mood decreases per second (bars per minute).")]
        public float moodDecayRate = 0.02f;

        [FormerlySerializedAs("BellyDecayRate")] [Tooltip("Rate at which belly decreases per second (bars per minute).")]
        public float bellyDecayRate = 0.03f;

        [FormerlySerializedAs("WeedMoodPenalty")] [Tooltip("Additional mood decay applied per weed present in the garden.")]
        public float weedMoodPenalty = 0.01f;

        [FormerlySerializedAs("RunAwayThreshold")] [Tooltip("Time in seconds that mood and belly must both be empty before run‑away.")]
        public float runAwayThreshold = 60f;

        private float runAwayTimer;
        private GardenManager garden;

        public event Action<ChaoStats> OnMoodChanged;
        public event Action<ChaoStats> OnBellyChanged;
        public event Action<ChaoController> OnRunAway;

        private void Awake()
        {
            garden = FindAnyObjectByType<GardenManager>();
            if (stats == null)
            {
                stats = new ChaoStats { name = "Chao" };
            }
        }

        private void Update()
        {
            var delta = Time.deltaTime;
            // Decrease mood and belly over time
            var moodDecay = moodDecayRate * delta;
            var bellyDecay = bellyDecayRate * delta;
            // Additional mood penalty from weeds
            var weedCount = garden != null ? garden.ActiveWeedCount : 0;
            moodDecay += weedCount * weedMoodPenalty * delta;
            UpdateMood(stats.mood - moodDecay);
            UpdateBelly(stats.belly - bellyDecay);

            if (stats.mood <= 0f && stats.belly <= 0f)
            {
                runAwayTimer += delta;
                if (runAwayTimer >= runAwayThreshold)
                {
                    TriggerRunAway();
                }
            }
            else
            {
                runAwayTimer = 0f;
            }
        }

        /// <summary>
        /// Player pets the Chao to raise its mood【908307047627800†L171-L175】.
        /// </summary>
        public void Pet()
        {
            UpdateMood(stats.mood + 0.2f);
        }

        /// <summary>
        /// Feed the Chao a piece of fruit and adjust stats accordingly【908307047627800†L171-L195】.
        /// </summary>
        public void Feed(FruitData fruit)
        {
            if (fruit == null) return;
            stats.ApplyFruit(fruit);
            OnMoodChanged?.Invoke(stats);
            OnBellyChanged?.Invoke(stats);
        }

        private void UpdateMood(float value)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(value, stats.mood)) return;
            stats.mood = value;
            OnMoodChanged?.Invoke(stats);
        }

        private void UpdateBelly(float value)
        {
            value = Mathf.Clamp01(value);
            if (Mathf.Approximately(value, stats.belly)) return;
            stats.belly = value;
            OnBellyChanged?.Invoke(stats);
        }

        private void TriggerRunAway()
        {
            // Remove the Chao from the garden and notify listeners【908307047627800†L184-L189】.
            OnRunAway?.Invoke(this);
            Destroy(gameObject);
        }
    }
}