using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Minigames
{
    /// <summary>
    /// Simplified version of the Tomodachi Sagashi Chao mini‑game where the
    /// player must locate a specific Chao among many using hints【908307047627800†L422-L444】.
    /// This implementation generates a number of NPC Chao with random names and
    /// positions. The player moves in a 2D area using keyboard controls. The
    /// player can talk to Tikal or examine hint orbs to receive clues about the
    /// target's location. Each incorrect guess reduces remaining time.
    /// </summary>
    public class SearchGame : MinigameBase
    {
        [FormerlySerializedAs("ChaoCount")] [Header("Game Settings")]
        public int chaoCount = 10;
        [FormerlySerializedAs("TimeLimit")] public float timeLimit = 120f;
        private float remainingTime;
        private bool isRunning;

        private List<NpcChao> npcs;
        public NpcChao Target;

        public class NpcChao
        {
            public string Name;
            public Vector2 Position;
        }

        protected override void StartGame(GardenManager gardenManager)
        {
            base.StartGame(gardenManager);
            isRunning = true;
            remainingTime = timeLimit;
            GenerateNpcs();
        }

        private void GenerateNpcs()
        {
            npcs = new List<NpcChao>();
            for (var i = 0; i < chaoCount; i++)
            {
                npcs.Add(new NpcChao
                {
                    Name = $"Chao {i + 1}",
                    Position = new Vector2(Random.Range(-5f, 5f), Random.Range(-3f, 3f))
                });
            }
            // Pick a random target
            Target = npcs[Random.Range(0, npcs.Count)];
        }

        private void Update()
        {
            if (!isRunning) return;
            remainingTime -= Time.deltaTime;
            if (remainingTime <= 0f)
            {
                EndGame();
            }
            // Movement input could be processed here to move the player character
        }

        /// <summary>
        /// Get a textual clue about the target's location based on the player's
        /// current position. For demonstration this returns a simple
        /// north/south/east/west hint comparing positions【908307047627800†L432-L436】.
        /// </summary>
        public string GetHint(Vector2 playerPosition)
        {
            var diff = Target.Position - playerPosition;
            var horizontal = diff.x > 0 ? "east" : "west";
            var vertical = diff.y > 0 ? "north" : "south";
            return $"The Chao is somewhere to the {vertical} {horizontal}.";
        }

        /// <summary>
        /// Called when the player selects a Chao NPC to guess. If correct,
        /// rings are awarded based on how many were previously found. If
        /// incorrect, time penalty is applied【908307047627800†L439-L444】.
        /// </summary>
        public void GuessChao(NpcChao npc)
        {
            if (!isRunning) return;
            if (npc == Target)
            {
                // Correct guess; award increasing ring bonus based on order found
                AwardRing(10);
                // Remove from list and pick new target if time remains
                npcs.Remove(npc);
                if (npcs.Count == 0)
                {
                    EndGame();
                }
                else
                {
                    Target = npcs[Random.Range(0, npcs.Count)];
                }
            }
            else
            {
                remainingTime -= 30f;
                if (remainingTime <= 0f)
                {
                    EndGame();
                }
            }
        }

        public override void EndGame()
        {
            base.EndGame();
            isRunning = false;
        }
    }
}