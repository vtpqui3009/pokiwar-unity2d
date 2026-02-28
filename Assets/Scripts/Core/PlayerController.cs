using UnityEngine;
using UnityEngine.InputSystem;

namespace Pokiwar.Core
{
    /// <summary>
    /// Handles player movement and input.
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

        public float MoveSpeed => moveSpeed;
        public Vector2 Velocity => rb.velocity;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            mainCamera = Camera.main;

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            HandleInput();
            UpdateFacingDirection();
            ClampToMapBounds();
        }

        private void FixedUpdate()
        {
            ApplyMovement();
            ApplyFriction();
        }

        private void HandleInput()
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

            moveInput = new Vector2(moveX, moveY).normalized;
        }

        private void ApplyMovement()
        {
            if (moveInput != Vector2.zero)
            {
                Vector2 targetVelocity = moveInput * moveSpeed;
                rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, acceleration * Time.fixedDeltaTime);
            }
        }

        private void ApplyFriction()
        {
            if (moveInput == Vector2.zero)
            {
                rb.velocity = Vector2.Lerp(rb.velocity, Vector2.zero, friction * Time.fixedDeltaTime);
            }
        }

        private void UpdateFacingDirection()
        {
            if (moveInput.x < 0)
                spriteRenderer.flipX = true;
            else if (moveInput.x > 0)
                spriteRenderer.flipX = false;
        }

        private void ClampToMapBounds()
        {
            if (GameManager.Instance != null)
            {
                Vector2 pos = transform.position;
                if (!GameManager.Instance.IsPositionInBounds(pos))
                {
                    float halfWidth = GameManager.Instance.MapWidth / 2f;
                    float halfHeight = GameManager.Instance.MapHeight / 2f;
                    pos.x = Mathf.Clamp(pos.x, -halfWidth, halfWidth);
                    pos.y = Mathf.Clamp(pos.y, -halfHeight, halfHeight);
                    transform.position = pos;
                }
            }
        }

        public void SetMoveSpeed(float speed)
        {
            moveSpeed = speed;
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
    }
}
