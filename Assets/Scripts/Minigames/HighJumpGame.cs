using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Minigames
{
    /// <summary>
    /// Simplified implementation of the Chao Super High‑Jump Game. The player
    /// controls a movable spring that launches the Chao (represented by a
    /// Rigidbody) into the air to collect rings. Rings award the player with
    /// rings to spend in the store【908307047627800†L369-L387】. After all rings
    /// are collected or lives are lost, the game ends.
    /// </summary>
    public class HighJumpGame : MinigameBase
    {
        [FormerlySerializedAs("ChaoRb")] [Header("References")]
        public Rigidbody chaoRb;
        [FormerlySerializedAs("Spring")] public Transform spring;
        [FormerlySerializedAs("RingPositions")] public List<Transform> ringPositions;
        [FormerlySerializedAs("LaunchForce")] public float launchForce = 10f;
        [FormerlySerializedAs("MoveSpeed")] public float moveSpeed = 3f;
        [FormerlySerializedAs("MaxLives")] public int maxLives = 3;

        private HashSet<Transform> ringsRemaining;
        private int lives;
        private bool isRunning;

        protected override void StartGame(GardenManager gardenManager)
        {
            base.StartGame(gardenManager);
            ringsRemaining = new HashSet<Transform>(ringPositions);
            lives = maxLives;
            isRunning = true;
            // Reset positions
            chaoRb.transform.position = spring.position + Vector3.up * 0.5f;
            chaoRb.linearVelocity = Vector3.zero;
        }

        private void Update()
        {
            if (!isRunning) return;
            // Move spring horizontally using left/right input
            var horizontal = Input.GetAxis("Horizontal");
            spring.Translate(Vector3.right * (horizontal * moveSpeed * Time.deltaTime));
            // Keep spring within bounds
            var pos = spring.position;
            pos.x = Mathf.Clamp(pos.x, -4f, 4f);
            spring.position = pos;
            // Launch when player presses space and Chao is on spring
            if (Input.GetKeyDown(KeyCode.Space))
            {
                LaunchChao();
            }
        }

        private void LaunchChao()
        {
            // Apply upward force to chao
            chaoRb.linearVelocity = new Vector3(0f, launchForce, 0f);
        }

        private void OnTriggerEnter(Collider other)
        {
            // When Chao collides with a ring transform, collect it
            if (!isRunning) return;
            if (ringsRemaining.Contains(other.transform))
            {
                ringsRemaining.Remove(other.transform);
                // Award ring(s). Each ring awards one ring from round 1; scaling is omitted for simplicity.
                AwardRing(1);
                other.gameObject.SetActive(false);
                if (ringsRemaining.Count == 0)
                {
                    EndGame();
                }
            }
            else if (other.transform == spring)
            {
                // If chao hits ground without landing on spring, lose a life
                lives--;
                if (lives <= 0)
                {
                    EndGame();
                }
                else
                {
                    // Reset chao to spring
                    chaoRb.transform.position = spring.position + Vector3.up * 0.5f;
                    chaoRb.linearVelocity = Vector3.zero;
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