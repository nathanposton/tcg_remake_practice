using UnityEngine;
using UnityEngine.InputSystem;

namespace Chao_Pet
{
    public class MovementLogicScript : MonoBehaviour
    {
        private static readonly int Facing1 = Animator.StringToHash("Facing");
        private static readonly int IsMoving = Animator.StringToHash("isMoving");
        private static readonly int MoveY = Animator.StringToHash("moveY");
        private static readonly int MoveX = Animator.StringToHash("moveX");

        public enum Facing { Down = 0, Up = 1, Left = 2, Right = 3 };
        [SerializeField] public float moveSpeed = 5;
        
        public Animator animator;
        private Rigidbody2D body;
        private SpriteRenderer spriteRenderer;
        
        [HideInInspector] public Vector2 movement;
        [HideInInspector] public Facing facing = Facing.Down;
        [HideInInspector] public bool isMoving;

        private void Awake()
        {
            body           = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator       = GetComponent<Animator>();
        }

        private void Update()
        {
            // --- INPUT & MOVE VECTOR ---
            movement = Vector2.zero;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                movement.x = -1f;
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                movement.x = 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                movement.y = -1f;
            else if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                movement.y = 1f;

            // --- DETERMINE FACING (4-way) ---
            isMoving = movement.magnitude > 0.1f;
            if (isMoving)
            {
                if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y))
                    facing = movement.x > 0 ? Facing.Right : Facing.Left;
                else
                    facing = movement.y > 0 ? Facing.Up : Facing.Down;
            } else {
                facing = Facing.Down;
            }

            // --- ANIMATOR PARAMS ---
            animator.SetFloat(MoveX, movement.x);
            animator.SetFloat(MoveY, movement.y);
            animator.SetBool(IsMoving, isMoving);
            animator.SetInteger(Facing1, (int)facing);

            // --- SPRITE FLIP (for left/right) ---
            spriteRenderer.flipX = (facing == Facing.Right);
        }

        private void FixedUpdate()
        {
            body.MovePosition(body.position + movement.normalized * (moveSpeed * Time.fixedDeltaTime));
        }
    }
}