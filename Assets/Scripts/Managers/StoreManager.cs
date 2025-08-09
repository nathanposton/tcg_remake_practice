using System.Collections.Generic;
using Components;
using Models;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Managers
{
    /// <summary>
    /// Manages the in‑game store where the player buys fruit, eggs and toys
    /// using rings【821806795223146†L520-L548】. The store panel can be toggled
    /// with the L button (handled elsewhere) and displays available items
    /// dynamically. Purchasing an item deducts rings from the Chao and
    /// applies the item in the garden (e.g. spawns fruit, egg or toy).
    /// </summary>
    public class StoreManager : MonoBehaviour
    {
        [FormerlySerializedAs("Fruits")] [Header("Item Catalogues")]
        public List<FruitData> fruits;
        [FormerlySerializedAs("Eggs")] public List<EggData> eggs;
        [FormerlySerializedAs("Toys")] public List<ToyData> toys;

        [FormerlySerializedAs("StorePanel")] [Header("UI References")]
        public GameObject storePanel;
        [FormerlySerializedAs("FruitContainer")] public Transform fruitContainer;
        [FormerlySerializedAs("EggContainer")] public Transform eggContainer;
        [FormerlySerializedAs("ToyContainer")] public Transform toyContainer;
        [FormerlySerializedAs("FruitButtonPrefab")] public GameObject fruitButtonPrefab;
        [FormerlySerializedAs("EggButtonPrefab")] public GameObject eggButtonPrefab;
        [FormerlySerializedAs("ToyButtonPrefab")] public GameObject toyButtonPrefab;

        [FormerlySerializedAs("Garden")] [Tooltip("Reference to the GardenManager for instantiating items.")]
        public GardenManager garden;

        private EggData currentRandomEgg;
        private bool freeEggAvailable;
        private bool tvAvailable;
        private void Start()
        {
            // Randomly select an egg for sale at the start of the session
            ChooseRandomEgg();
            PopulateStoreUI();
        }

        private void ChooseRandomEgg()
        {
            if (eggs == null || eggs.Count == 0) return;
            // Weighted random selection based on rarity
            var totalWeight = 0f;
            foreach (var egg in eggs)
            {
                totalWeight += Mathf.Max(0.01f, 1f - egg.rarity); // rarer eggs have lower weight
            }
            var random = Random.Range(0f, totalWeight);
            foreach (var egg in eggs)
            {
                random -= Mathf.Max(0.01f, 1f - egg.rarity);
                if (random <= 0f)
                {
                    currentRandomEgg = egg;
                    break;
                }
            }
        }

        private void PopulateStoreUI()
        {
            if (fruitButtonPrefab == null || eggButtonPrefab == null || toyButtonPrefab == null)
                return;

            // Clear previous buttons
            foreach (Transform child in fruitContainer) Destroy(child.gameObject);
            foreach (Transform child in eggContainer) Destroy(child.gameObject);
            foreach (Transform child in toyContainer) Destroy(child.gameObject);

            // Create fruit buttons
            foreach (var fruit in fruits)
            {
                var go = Instantiate(fruitButtonPrefab, fruitContainer);
                var btn = go.GetComponent<Button>();
                btn.onClick.AddListener(() => PurchaseFruit(fruit));
                var text = go.GetComponentInChildren<Text>();
                if (text != null)
                    text.text = $"{fruit.itemName}\n{fruit.cost} Rings";
            }

            // Create egg buttons. Show free egg if available otherwise show random egg.
            if (freeEggAvailable)
            {
                var freeEgg = eggs.Find(e => e.itemName.ToLower().Contains("normal"));
                var go = Instantiate(eggButtonPrefab, eggContainer);
                var btn = go.GetComponent<Button>();
                btn.onClick.AddListener(() => PurchaseEgg(freeEgg));
                var text = go.GetComponentInChildren<Text>();
                text.text = $"{freeEgg.itemName}\nFree";
            }
            else if (currentRandomEgg != null)
            {
                var go = Instantiate(eggButtonPrefab, eggContainer);
                var btn = go.GetComponent<Button>();
                btn.onClick.AddListener(() => PurchaseEgg(currentRandomEgg));
                var text = go.GetComponentInChildren<Text>();
                text.text = $"{currentRandomEgg.itemName}\n{currentRandomEgg.cost} Rings";
            }

            // Create toy buttons in order. Do not show TV until after buying duck and waiting 3 hours【908307047627800†L270-L280】.
            foreach (var toy in toys)
            {
                if (!tvAvailable && toy.itemName.ToLower().Contains("tv"))
                    continue;
                var go = Instantiate(toyButtonPrefab, toyContainer);
                var btn = go.GetComponent<Button>();
                btn.onClick.AddListener(() => PurchaseToy(toy));
                var text = go.GetComponentInChildren<Text>();
                text.text = $"{toy.itemName}\n{toy.cost} Rings";
            }
        }

        public void ToggleStore()
        {
            if (storePanel != null)
                storePanel.SetActive(!storePanel.activeSelf);
        }

        private bool HasActiveEggOrChao()
        {
            return garden.activeChao != null || garden.transform.Find("Egg") != null;
        }

        private void PurchaseFruit(FruitData fruit)
        {
            var stats = garden.activeChao?.stats;
            if (stats == null || stats.rings < fruit.cost) return;
            stats.rings -= fruit.cost;
            // Instantiate fruit in the garden as a pick‑up
            SpawnFruitInGarden(fruit);
            garden.statsPanel.UpdateRings(stats.rings);
        }

        private void SpawnFruitInGarden(FruitData fruit)
        {
            // For simplicity create a sphere that represents the fruit with a collider.
            var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = new Vector3(Random.Range(-2f, 2f), 0f, Random.Range(-1f, 1f));
            sphere.name = fruit.itemName;
            var sphereCollider = sphere.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            var pickup = sphere.AddComponent<FruitPickup>();
            pickup.data = fruit;
            pickup.garden = garden;
        }

        private void PurchaseEgg(EggData egg)
        {
            var stats = garden.activeChao?.stats;
            var rings = stats != null ? stats.rings : 0;
            var cost = freeEggAvailable ? 0 : egg.cost;
            if (HasActiveEggOrChao() || rings < cost) return;
            if (stats != null)
                stats.rings -= cost;
            SpawnEgg(egg);
            garden.statsPanel.UpdateRings(stats?.rings ?? 0);
            freeEggAvailable = false;
            PopulateStoreUI();
        }

        private void SpawnEgg(EggData egg)
        {
            var obj = new GameObject("Egg");
            obj.transform.position = new Vector3(0f, 0f, 0f);
            var eggComp = obj.AddComponent<Egg>();
            eggComp.data = egg;
            eggComp.garden = garden;
        }

        private void PurchaseToy(ToyData toy)
        {
            var stats = garden.activeChao?.stats;
            if (stats == null || stats.rings < toy.cost) return;
            stats.rings -= toy.cost;
            garden.statsPanel.UpdateRings(stats.rings);
            // Instantiate toy prefab into garden
            Instantiate(toy.toyPrefab, new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)), Quaternion.identity, garden.transform);
            // If the toy is the duck, unlock the TV after 3 in‑game hours. Use a coroutine.
            if (toy.itemName.ToLower().Contains("duck"))
            {
                StartCoroutine(UnlockTVAfterHours(3f * 60f * 60f));
            }
        }

        private System.Collections.IEnumerator UnlockTVAfterHours(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            tvAvailable = true;
            PopulateStoreUI();
        }

        /// <summary>
        /// Called by GardenManager when a Chao runs away to offer a free normal egg【908307047627800†L184-L189】.
        /// </summary>
        public void SpawnFreeEgg()
        {
            freeEggAvailable = true;
            PopulateStoreUI();
        }
    }

    /// <summary>
    /// Component representing a pickup fruit in the garden. When the Chao walks
    /// into the collider the fruit is consumed and the Chao's stats are updated.
    /// </summary>
    public class FruitPickup : MonoBehaviour
    {
        [FormerlySerializedAs("Data")] public FruitData data;
        [FormerlySerializedAs("Garden")] public GardenManager garden;
        private void OnTriggerEnter(Collider other)
        {
            var chao = other.GetComponent<ChaoController>();
            if (chao != null && data != null)
            {
                chao.Feed(data);
                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Represents an egg placed in the garden. It will hatch after a delay
    /// unless the player pets it to speed the process【908307047627800†L153-L159】.
    /// When hatching it spawns a Chao.
    /// </summary>
    public class Egg : MonoBehaviour
    {
        [FormerlySerializedAs("Data")] public EggData data;
        [FormerlySerializedAs("Garden")] public GardenManager garden;
        [FormerlySerializedAs("HatchTime")] public float hatchTime = 10f;
        private float timer;
        private bool isHatching;
        private void Update()
        {
            timer += Time.deltaTime;
            if (timer >= hatchTime)
            {
                Hatch();
            }
        }
        private void OnMouseDown()
        {
            // Petting the egg speeds up hatching【908307047627800†L153-L159】.
            timer += hatchTime * 0.1f;
        }
        private void Hatch()
        {
            if (isHatching) return;
            isHatching = true;
            // Instantiate new Chao
            var chaoObj = new GameObject("Chao");
            chaoObj.transform.position = transform.position;
            var controller = chaoObj.AddComponent<ChaoController>();
            controller.stats = new ChaoStats
            {
                name = data.isJewel ? $"Jewel {data.itemName}" : "Chao",
                stage = ChaoStage.Child,
                rings = 0
            };
            garden.activeChao = controller;
            garden.statsPanel.Initialise(controller.stats);
            // Clean up egg
            Destroy(gameObject);
        }
    }
}