using UnityEngine;
using UnityEngine.Serialization;

namespace Models
{
    /// <summary>
    /// Base class for store items (fruits, eggs, toys).
    /// Items are implemented as ScriptableObjects so they can be authored
    /// directly in the Unity editor and referenced without hard‑coding values.
    /// </summary>
    public abstract class ItemData : ScriptableObject
    {
        [FormerlySerializedAs("ItemName")] [Tooltip("Name shown in the store UI.")]
        public string itemName;

        [FormerlySerializedAs("Cost")] [Tooltip("Cost in rings required to purchase this item.")]
        public int cost;
    }
}