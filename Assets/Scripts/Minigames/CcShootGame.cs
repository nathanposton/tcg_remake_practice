using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Minigames
{
    /// <summary>
    /// Simplified version of the Chao's CC Shoot mini‑game from Sonic Pinball
    /// Party【908307047627800†L389-L419】. Chips of two colours descend from the top
    /// of the screen. The player fires chips of either colour upward to make
    /// groups of 10 which are removed, awarding rings. If any chip reaches
    /// the bottom area the game ends.
    /// </summary>
    public class CcShootGame : MinigameBase
    {
        [FormerlySerializedAs("Columns")] public int columns = 8;
        [FormerlySerializedAs("Rows")] public int rows = 10;
        private int[,] grid; // 0 = empty, 1 = red, 2 = green
        [FormerlySerializedAs("FallInterval")] public float fallInterval = 2f;
        private float fallTimer;
        private bool isRunning;

        protected override void StartGame(GardenManager gardenManager)
        {
            base.StartGame(gardenManager);
            grid = new int[rows, columns];
            isRunning = true;
            fallTimer = fallInterval;
        }

        private void Update()
        {
            if (!isRunning) return;
            fallTimer -= Time.deltaTime;
            if (fallTimer <= 0f)
            {
                fallTimer = fallInterval;
                AdvanceRow();
            }
            // Inputs: keys to shoot red (A) or green (B)
            if (Input.GetKeyDown(KeyCode.A))
            {
                FireChip(1);
            }
            else if (Input.GetKeyDown(KeyCode.S))
            {
                FireChip(2);
            }
        }

        private void AdvanceRow()
        {
            // Move all rows down
            for (var r = rows - 1; r > 0; r--)
            {
                for (var c = 0; c < columns; c++)
                {
                    grid[r, c] = grid[r - 1, c];
                }
            }
            // Spawn new random row at top
            for (var c = 0; c < columns; c++)
            {
                grid[0, c] = Random.Range(1, 3);
            }
            // Check if any chip reached bottom row (game over)
            for (var c = 0; c < columns; c++)
            {
                if (grid[rows - 1, c] != 0)
                {
                    EndGame();
                    return;
                }
            }
        }

        private void FireChip(int color)
        {
            // Drop a chip into the column that currently aligns with the chao. For simplicity use middle column.
            var column = columns / 2;
            // Find highest empty cell in that column
            for (var r = rows - 1; r >= 0; r--)
            {
                if (grid[r, column] == 0)
                {
                    grid[r, column] = color;
                    CheckForMatches(r, column, color);
                    break;
                }
            }
        }

        private void CheckForMatches(int row, int column, int color)
        {
            // Flood fill to count connected chips of same colour
            var visited = new bool[rows, columns];
            var cluster = new List<(int r, int c)>();
            FloodFill(row, column, color, visited, cluster);
            if (cluster.Count >= 10)
            {
                // Remove cluster and award rings equal to cluster size
                foreach (var cell in cluster)
                {
                    grid[cell.r, cell.c] = 0;
                }
                AwardRing(cluster.Count);
            }
        }

        private void FloodFill(int r, int c, int color, bool[,] visited, List<(int r, int c)> cluster)
        {
            if (r < 0 || r >= rows || c < 0 || c >= columns) return;
            if (visited[r, c] || grid[r, c] != color) return;
            visited[r, c] = true;
            cluster.Add((r, c));
            // Explore orthogonal and diagonal neighbours【908307047627800†L406-L417】
            for (var dr = -1; dr <= 1; dr++)
            {
                for (var dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    FloodFill(r + dr, c + dc, color, visited, cluster);
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