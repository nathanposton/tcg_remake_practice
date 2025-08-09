using UnityEngine;

namespace Chao_Pet
{
    public class EmoteBallFollow : MonoBehaviour
    {
        public Vector2 downOffset  = new Vector2( 0f, 1.5f);
        public Vector2 upOffset    = new Vector2( 0f, 2.0f);
        public Vector2 leftOffset  = new Vector2(-0.5f, 1.7f);
        public Vector2 rightOffset = new Vector2( 0.5f, 1.7f);

        public float bobAmplitude = 0.1f;
        public float bobFrequency = 2f;

        public float springStiffness = 200f;
        public float springDamping   = 20f;

        private Transform playerT;
        private MovementLogicScript movementScript;
        private SpriteRenderer playerSr;
        private SpriteRenderer ballSr;
        private Vector3 velocity;

        private void Awake()
        {
            playerT  = transform.parent;
            movementScript = playerT.GetComponent<MovementLogicScript>();
            playerSr = playerT.GetComponent<SpriteRenderer>();
            ballSr   = GetComponentInChildren<SpriteRenderer>();
        }

        private void Update()
        {
            // grab "isMoving" from the MovementLogicScript.
            var isMoving = movementScript.isMoving;

            // grab the raw Facing, but override to Down if we're idle
            var rawFacing = movementScript.facing;
            var f = isMoving ? rawFacing : MovementLogicScript.Facing.Down;

            // now pick offsets based on f…
            var baseOffset = f switch
            {
                MovementLogicScript.Facing.Up => upOffset,
                MovementLogicScript.Facing.Left => leftOffset,
                MovementLogicScript.Facing.Right => rightOffset,
                _ => downOffset
            };

            // 2) Bob
            var bobY = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            var targetLocal = new Vector3(baseOffset.x, baseOffset.y + bobY, 0f);

            // 3) Spring‐damper physics
            var stretch      = transform.localPosition - targetLocal;
            var springForce  = -springStiffness * stretch;
            var dampingForce = -springDamping   * velocity;
            var acceleration = springForce + dampingForce;

            velocity               += acceleration * Time.deltaTime;
            transform.localPosition += velocity * Time.deltaTime;

            // 4) Swap sorting order
            // Facing.Down (0) => behind; else => in front
            ballSr.sortingOrder = (f == MovementLogicScript.Facing.Down)
                ? playerSr.sortingOrder - 1
                : playerSr.sortingOrder + 1;
        }
    }
}