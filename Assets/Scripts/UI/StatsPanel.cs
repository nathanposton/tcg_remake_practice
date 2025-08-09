using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// Displays the Chao's statistics on the right side of the garden screen【821806795223146†L530-L548】.
    /// Each stat is represented by a slider or text. This script exposes
    /// methods to update UI elements in response to changes in the Chao
    /// controller.
    /// </summary>
    public class StatsPanel : MonoBehaviour
    {
        [FormerlySerializedAs("MoodSlider")] [Header("Sliders")]
        public Slider moodSlider;
        [FormerlySerializedAs("BellySlider")] public Slider bellySlider;
        [FormerlySerializedAs("SwimSlider")] public Slider swimSlider;
        [FormerlySerializedAs("FlySlider")] public Slider flySlider;
        [FormerlySerializedAs("RunSlider")] public Slider runSlider;
        [FormerlySerializedAs("PowerSlider")] public Slider powerSlider;
        [FormerlySerializedAs("StaminaSlider")] public Slider staminaSlider;

        [FormerlySerializedAs("NameText")] [Header("Text Fields")]
        public Text nameText;
        [FormerlySerializedAs("StageText")] public Text stageText;
        [FormerlySerializedAs("RingsText")] public Text ringsText;

        public void Initialise(Models.ChaoStats stats)
        {
            UpdateAll(stats);
        }

        public void UpdateAll(Models.ChaoStats stats)
        {
            if (stats == null) return;
            nameText.text = stats.name;
            stageText.text = stats.stage.ToString();
            UpdateMood(stats.mood);
            UpdateBelly(stats.belly);
            UpdateAbilityStats(stats);
            UpdateRings(stats.rings);
        }

        public void UpdateMood(float value)
        {
            if (moodSlider != null)
            {
                moodSlider.value = value;
            }
        }

        public void UpdateBelly(float value)
        {
            if (bellySlider != null)
            {
                bellySlider.value = value;
            }
        }

        public void UpdateAbilityStats(Models.ChaoStats stats)
        {
            if (stats == null) return;
            swimSlider.value = stats.swim / 99f;
            flySlider.value = stats.fly / 99f;
            runSlider.value = stats.run / 99f;
            powerSlider.value = stats.power / 99f;
            staminaSlider.value = stats.stamina / 99f;
        }

        public void UpdateRings(int rings)
        {
            ringsText.text = rings.ToString();
        }
    }
}