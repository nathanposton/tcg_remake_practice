using System.Collections.Generic;
using Components;
using Models;
using UnityEngine;
using UnityEngine.Serialization;

namespace Managers
{
    /// <summary>
    /// Manages the Tiny Chao Garden scene. Responsible for spawning weeds,
    /// tracking the active Chao, handling ring totals and interfacing with the
    /// store and minigames. Attach this to an empty GameObject in the garden
    /// scene.
    /// </summary>
    public class GardenManager : MonoBehaviour
    {
        [FormerlySerializedAs("WeedPrefab")]
        [Header("Weed Settings")]
        [Tooltip("Prefab representing a weed that can be plucked by the player.")]
        public GameObject weedPrefab;
        [FormerlySerializedAs("WeedSpawnInterval")] [Tooltip("Time interval in seconds between weed spawns.")]
        public float weedSpawnInterval = 30f;
        [FormerlySerializedAs("MaxWeeds")] [Tooltip("Maximum number of weeds allowed to spawn at once.")]
        public int maxWeeds = 5;
        [FormerlySerializedAs("WeedSpawnMin")] [Tooltip("Random spawn area bounds for weeds (x,z plane).")]
        public Vector2 weedSpawnMin = new Vector2(-2f, -1f);
        [FormerlySerializedAs("WeedSpawnMax")] public Vector2 weedSpawnMax = new Vector2(2f, 1f);

        private readonly List<GameObject> weeds = new List<GameObject>();
        private float weedTimer;

        public int ActiveWeedCount => weeds.Count;

        [FormerlySerializedAs("ActiveChao")] [Header("References")]
        public ChaoController activeChao;
        [FormerlySerializedAs("Store")] public StoreManager store;
        [FormerlySerializedAs("StatsPanel")] public UI.StatsPanel statsPanel;

        private void Start()
        {
            if (activeChao != null)
            {
                activeChao.OnMoodChanged += HandleMoodChanged;
                activeChao.OnBellyChanged += HandleBellyChanged;
                activeChao.OnRunAway += HandleChaoRanAway;
            }
        }

        private void Update()
        {
            // Spawn weeds periodically
            weedTimer += Time.deltaTime;
            if (weedTimer >= weedSpawnInterval && weeds.Count < maxWeeds)
            {
                weedTimer = 0f;
                SpawnWeed();
            }
        }

        private void SpawnWeed()
        {
            if (weedPrefab == null) return;
            var pos = new Vector3(
                Random.Range(weedSpawnMin.x, weedSpawnMax.x),
                0f,
                Random.Range(weedSpawnMin.y, weedSpawnMax.y));
            var weed = Instantiate(weedPrefab, pos, Quaternion.identity, transform);
            var weedComp = weed.GetComponent<Weed>();
            weedComp?.Init(this);
            weeds.Add(weed);
        }

        /// <summary>
        /// Called by a Weed when it is plucked. Removes it from the active list.
        /// </summary>
        public void RemoveWeed(GameObject weed)
        {
            weeds.Remove(weed);
        }

        private void HandleMoodChanged(ChaoStats stats)
        {
            statsPanel?.UpdateMood(stats.mood);
        }

        private void HandleBellyChanged(ChaoStats stats)
        {
            statsPanel?.UpdateBelly(stats.belly);
        }

        private void HandleChaoRanAway(ChaoController chao)
        {
            // When the Chao runs away, we can spawn a free egg in the store【908307047627800†L184-L189】.
            store?.SpawnFreeEgg();
        }
    }
}