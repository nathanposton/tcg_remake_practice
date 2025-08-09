using Models;
using UnityEngine;
using UnityEngine.Serialization;

namespace Saving
{
    /// <summary>
    /// Provides simple persistence of the player's Chao and ring data using
    /// PlayerPrefs. The Tiny Chao Garden on Game Boy Advance required the
    /// player to use the Save & Exit option to record progress【908307047627800†L115-L118】.
    /// Similarly, call Save() in Unity when the player exits the garden.
    /// </summary>
    public static class SaveSystem
    {
        private const string ChaoKey = "TinyChaoGarden_SaveData";

        [System.Serializable]
        private class SaveData
        {
            [FormerlySerializedAs("Stats")] public ChaoStats stats;
        }

        public static void SaveChao(ChaoStats stats)
        {
            var data = new SaveData { stats = stats };
            var json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(ChaoKey, json);
            PlayerPrefs.Save();
        }

        public static ChaoStats LoadChao()
        {
            if (!PlayerPrefs.HasKey(ChaoKey)) return null;
            var json = PlayerPrefs.GetString(ChaoKey);
            var data = JsonUtility.FromJson<SaveData>(json);
            return data.stats;
        }

        public static void ClearSave()
        {
            PlayerPrefs.DeleteKey(ChaoKey);
        }
    }
}