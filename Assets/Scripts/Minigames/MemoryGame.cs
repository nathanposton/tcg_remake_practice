using System.Collections.Generic;
using Managers;
using UnityEngine;

namespace Minigames
{
    /// <summary>
    /// Memory match mini‑game where the player flips over pairs of cards to
    /// earn rings【908307047627800†L446-L474】. Ring rewards depend on the card's
    /// position on the board. This class manages the card state and ring
    /// awarding logic. UI should call FlipCard() when the player selects a
    /// card at a given index.
    /// </summary>
    public class MemoryGame : MinigameBase
    {
        private class Card
        {
            public int Type; // 0–6 index of the food
            public bool Matched;
            public int RingValue;
        }

        private List<Card> cards;
        private List<int> selectedIndices;
        private int mistakesLeft;
        private bool isRunning;

        /// <summary>
        /// Board layout ring values corresponding to each position (0–13). These
        /// values represent the outer (1 ring), middle (3 rings) and inner
        /// (5 rings) layers of the board as described in the Tiny Chao Garden
        /// memory game【908307047627800†L464-L474】.
        /// </summary>
        private static readonly int[] PositionRingValues = new int[]
        {
            1,1,1,1,1,
            3,3,3,3,
            5,5,5,5,5
        };

        protected override void StartGame(GardenManager gardenManager)
        {
            base.StartGame(gardenManager);
            mistakesLeft = 3;
            isRunning = true;
            InitialiseBoard();
        }

        private void InitialiseBoard()
        {
            // Create 7 types (0–6) each with two cards
            var temp = new List<Card>();
            for (var type = 0; type < 7; type++)
            {
                for (var i = 0; i < 2; i++)
                {
                    temp.Add(new Card { Type = type, Matched = false });
                }
            }
            // Randomly choose 14 cards (all of them) and assign ring values
            cards = new List<Card>(14);
            var index = 0;
            while (temp.Count > 0)
            {
                var idx = Random.Range(0, temp.Count);
                var card = temp[idx];
                temp.RemoveAt(idx);
                card.RingValue = PositionRingValues[index];
                cards.Add(card);
                index++;
            }
            selectedIndices = new List<int>(2);
        }

        /// <summary>
        /// Flip a card at the given index. If two cards are selected the game
        /// checks for a match and awards rings accordingly. UI should disable
        /// selecting the same card twice or flipping matched cards.
        /// </summary>
        public void FlipCard(int index)
        {
            if (!isRunning || index < 0 || index >= cards.Count) return;
            var card = cards[index];
            if (card.Matched) return;
            if (selectedIndices.Contains(index)) return;
            selectedIndices.Add(index);
            if (selectedIndices.Count == 2)
            {
                ResolveSelection();
            }
        }

        private void ResolveSelection()
        {
            var firstIndex = selectedIndices[0];
            var secondIndex = selectedIndices[1];
            var first = cards[firstIndex];
            var second = cards[secondIndex];
            if (first.Type == second.Type)
            {
                // Match: award rings based on position values
                var reward = first.RingValue + second.RingValue;
                AwardRing(reward);
                first.Matched = true;
                second.Matched = true;
                // Check for game complete
                if (IsComplete())
                {
                    var bonus = mistakesLeft == 1 ? 10 : mistakesLeft == 2 ? 30 : 60;
                    AwardRing(bonus);
                    EndGame();
                }
            }
            else
            {
                mistakesLeft--;
                if (mistakesLeft <= 0)
                {
                    EndGame();
                }
            }
            selectedIndices.Clear();
        }

        private bool IsComplete()
        {
            foreach (var card in cards)
            {
                if (!card.Matched) return false;
            }
            return true;
        }

        public override void EndGame()
        {
            base.EndGame();
            isRunning = false;
            // Return control to the garden scene. Implementation specific.
        }
    }
}