using UnityEngine;
using UnityEngine.InputSystem;

namespace Pokiwar.Core
{
    /// <summary>
    /// Handles player movement and input. Supports keyboard and mobile joystick input.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float friction = 5f;

        [Header("Components")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Rigidbody2D rb;
        private Vector2 moveInput;
        private Camera mainCamera;
        private float baseSpeed;

        public float MoveSpeed => moveSpeed;
        public float BaseSpeed => baseSpeed;
        public Vector2 Velocity => rb != null ? rb.linearVelocity : Vector2.zero;
        public bool IsMoving => moveInput != Vector2.zero;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            mainCamera = Camera.main;
            baseSpeed = moveSpeed;

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            HandleKeyboardInput();
            UpdateFacingDirection();
            ClampToMapBounds();
        }

        private void FixedUpdate()
        {
            ApplyMovement();
            ApplyFriction();
        }

        private void HandleKeyboardInput()
        {
            float moveX = 0f;
            float moveY = 0f;

            if (Keyboard.current != null)
            {
                if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
                    moveY = 1f;
                if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
                    moveY = -1f;
                if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                    moveX = -1f;
                if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                    moveX = 1f;
            }

            // Only override if keyboard has input (mobile joystick can also set moveInput)
            if (moveX != 0f || moveY != 0f)
                moveInput = new Vector2(moveX, moveY).normalized;
        }

        private void ApplyMovement()
        {
            if (moveInput != Vector2.zero)
            {
                Vector2 targetVelocity = moveInput * moveSpeed;
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            }
        }

        private void ApplyFriction()
        {
            if (moveInput == Vector2.zero)
            {
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, friction * Time.fixedDeltaTime);
            }
        }

        private void UpdateFacingDirection()
        {
            if (spriteRenderer == null) return;

            if (moveInput.x < 0)
                spriteRenderer.flipX = true;
            else if (moveInput.x > 0)
                spriteRenderer.flipX = false;
        }

        private void ClampToMapBounds()
        {
            if (GameManager.Instance == null) return;

            Vector2 pos = transform.position;
            if (!GameManager.Instance.IsPositionInBounds(pos))
            {
                transform.position = GameManager.Instance.ClampToBounds(pos);
            }
        }

        public void SetMoveSpeed(float speed)
        {
            moveSpeed = Mathf.Max(0f, speed);
        }

        public void ResetMoveSpeed()
        {
            moveSpeed = baseSpeed;
        }

        public void SetBaseSpeed(float speed)
        {
            baseSpeed = Mathf.Max(0f, speed);
            moveSpeed = baseSpeed;
        }

        public void SetSprite(Sprite sprite)
        {
            if (spriteRenderer != null && sprite != null)
                spriteRenderer.sprite = sprite;
        }

        public void SetInput(Vector2 input)
        {
            moveInput = input.normalized;
        }

        public void ClearInput()
        {
            moveInput = Vector2.zero;
        }
    }
}
