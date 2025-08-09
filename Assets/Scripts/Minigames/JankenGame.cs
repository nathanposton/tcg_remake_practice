using System.Collections.Generic;
using Managers;
using UnityEngine;

namespace Minigames
{
    /// <summary>
    /// Implementation of the Janken mini‑game (Rock/Paper/Scissors). Cards
    /// representing Rock, Paper or Scissors fly around and the player can
    /// shoot their own cards to clear them, earning rings【908307047627800†L318-L360】.
    /// This script focuses on the core game logic; UI should be implemented in
    /// Unity using buttons or touch controls that call ShootCard().
    /// </summary>
    public class JankenGame : MinigameBase
    {
        public enum CardType { Rock, Paper, Scissors }

        private readonly List<CardType> flyingCards = new List<CardType>();
        private readonly Queue<CardType> playerCards = new Queue<CardType>();
        private int stock = 5;
        private float timer;
        public float timeLimit = 30f;
        private int setsCleared;
        private bool isRunning;

        protected override void StartGame(GardenManager gardenManager)
        {
            base.StartGame(gardenManager);
            // Initialize with a random distribution of 5 flying cards and 3 player cards
            flyingCards.Clear();
            for (var i = 0; i < 5; i++)
            {
                flyingCards.Add(RandomCard());
            }
            playerCards.Clear();
            for (var i = 0; i < 3; i++)
            {
                playerCards.Enqueue(RandomCard());
            }
            timer = timeLimit;
            stock = 5;
            setsCleared = 0;
            isRunning = true;
        }

        private CardType RandomCard()
        {
            return (CardType)Random.Range(0, 3);
        }

        private void Update()
        {
            if (!isRunning) return;
            timer -= Time.deltaTime;
            if (timer <= 0f || (playerCards.Count == 0 && stock == 0))
            {
                EndGame();
            }
        }

        /// <summary>
        /// Called by UI when the player selects a card to shoot. The chosen card
        /// is compared against the first flying card. Rings are awarded or not
        /// based on the result【908307047627800†L320-L350】.
        /// </summary>
        public void ShootCard(CardType chosen)
        {
            if (!isRunning || playerCards.Count == 0) return;
            // Remove one card from player's queue
            playerCards.Dequeue();
            var target = flyingCards[0];
            var removeTarget = false;
            var awardRing = false;
            if (Beats(chosen, target))
            {
                removeTarget = true;
                awardRing = true;
            }
            else if (chosen == target)
            {
                removeTarget = true;
            }
            // else lose card; nothing happens to target
            if (removeTarget)
            {
                flyingCards.RemoveAt(0);
                // Award rings based on sets cleared【908307047627800†L350-L356】
                var ringValue = setsCleared >= 6 ? 4 : setsCleared >= 4 ? 3 : setsCleared >= 2 ? 2 : 1;
                if (awardRing)
                {
                    AwardRing(ringValue);
                }
                // If all flying cards cleared, spawn new set and extend timer
                if (flyingCards.Count == 0)
                {
                    setsCleared++;
                    for (var i = 0; i < 5; i++)
                        flyingCards.Add(RandomCard());
                    timer += 10f;
                }
            }
            // Replenish player's card if stock remains
            if (stock > 0)
            {
                playerCards.Enqueue(RandomCard());
                stock--;
            }
        }

        private bool Beats(CardType attack, CardType defense)
        {
            return (attack == CardType.Rock && defense == CardType.Scissors) ||
                   (attack == CardType.Paper && defense == CardType.Rock) ||
                   (attack == CardType.Scissors && defense == CardType.Paper);
        }

        public override void EndGame()
        {
            base.EndGame();
            isRunning = false;
            // Clean up and exit back to garden. Implementation details depend on UI.
            // This script simply stops awarding rings; controlling script can close the minigame scene.
        }
    }
}