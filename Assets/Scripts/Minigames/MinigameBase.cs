using Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Minigames
{
    /// <summary>
    /// Base class for all Tiny Chao Garden mini‑games. Provides common
    /// functionality such as awarding rings and ending the game. A reference
    /// to the GardenManager allows each mini‑game to deposit rings back to
    /// the Chao's stats【821806795223146†L520-L548】.
    /// </summary>
    public abstract class MinigameBase : MonoBehaviour
    {
        [FormerlySerializedAs("Garden")] public GardenManager garden;
        protected int RingsEarned;

        protected virtual void StartGame(GardenManager gardenManager)
        {
            this.garden = gardenManager;
            RingsEarned = 0;
        }

        protected void AwardRing(int amount)
        {
            if (garden != null && garden.activeChao != null)
            {
                garden.activeChao.stats.rings += amount;
                garden.statsPanel.UpdateRings(garden.activeChao.stats.rings);
            }
        }

        public virtual void EndGame()
        {
            // Called when the player quits or the mini‑game ends
            // Derived classes can override to perform cleanup
        }
    }
}