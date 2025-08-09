using Managers;
using UnityEngine;

namespace Components
{
    /// <summary>
    /// Represents a weed that appears randomly in the garden. When clicked,
    /// it is removed, raising the Chao's mood indirectly by reducing the
    /// negative mood decay applied in the GardenManager【908307047627800†L171-L175】.
    /// </summary>
    public class Weed : MonoBehaviour
    {
        private GardenManager garden;

        /// <summary>
        /// Initialise the weed with a reference to the garden for callback.
        /// </summary>
        public void Init(GardenManager manager)
        {
            garden = manager;
        }

        private void OnMouseDown()
        {
            // When the player clicks (taps) the weed, remove it.
            garden?.RemoveWeed(gameObject);
            Destroy(gameObject);
        }
    }
}