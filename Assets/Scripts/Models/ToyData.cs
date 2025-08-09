using UnityEngine;
using UnityEngine.Serialization;

namespace Models
{
    /// <summary>
    /// Toys can be bought to enrich the Chao's environment. They do not
    /// directly modify stats but affect the Chao's behaviour. For example,
    /// the trumpet allows the Chao to play music and the television keeps it
    /// entertained【908307047627800†L270-L280】.
    /// </summary>
    [CreateAssetMenu(fileName = "ToyData", menuName = "TinyChaoGarden/Toy", order = 3)]
    public class ToyData : ItemData
    {
        [FormerlySerializedAs("ToyPrefab")] [Tooltip("Prefab representing the toy placed in the garden.")]
        public GameObject toyPrefab;
    }
}