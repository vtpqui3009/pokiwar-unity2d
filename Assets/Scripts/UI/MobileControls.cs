using UnityEngine;
using UnityEngine.UI;
using Pokiwar.Core;

namespace Pokiwar.UI
{
    /// <summary>
    /// Mobile touch controls for on-screen joystick.
    /// </summary>
    public class MobileControls : MonoBehaviour
    {
        [Header("Virtual Joystick")]
        [SerializeField] private Image joystickBackground;
        [SerializeField] private Image joystickHandle;
        [SerializeField] private float joystickRange = 50f;

        [Header("Boost Button")]
        [SerializeField] private Button boostButton;

        private Vector2 joystickCenter;
        private bool isDragging;
        private PlayerController localPlayer;
        private Vector2 moveInput;

        private void Start()
        {
            if (joystickBackground != null)
            {
                joystickCenter = joystickBackground.rectTransform.anchoredPosition;
            }

            if (boostButton != null)
            {
                boostButton.onClick.AddListener(OnBoostPressed);
            }
        }

        private void Update()
        {
            if (localPlayer == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    localPlayer = player.GetComponent<PlayerController>();
                }
            }

            if (localPlayer != null && moveInput != Vector2.zero)
            {
                localPlayer.SetInput(moveInput);
            }
        }

        public void OnJoystickDrag(Vector2 position)
        {
            if (joystickHandle == null)
                return;

            isDragging = true;

            Vector2 offset = position - joystickCenter;
            offset = Vector2.ClampMagnitude(offset, joystickRange);

            joystickHandle.rectTransform.anchoredPosition = joystickCenter + offset;

            moveInput = offset / joystickRange;
        }

        public void OnJoystickRelease()
        {
            isDragging = false;

            if (joystickHandle != null)
            {
                joystickHandle.rectTransform.anchoredPosition = joystickCenter;
            }

            moveInput = Vector2.zero;

            if (localPlayer != null)
            {
                localPlayer.SetInput(Vector2.zero);
            }
        }

        private void OnBoostPressed()
        {
            if (localPlayer != null)
            {
                float currentSpeed = localPlayer.MoveSpeed;
                localPlayer.SetMoveSpeed(currentSpeed * 1.5f);
                Invoke(nameof(ResetSpeed), 0.5f);
            }
        }

        private void ResetSpeed()
        {
            if (localPlayer != null)
            {
                localPlayer.SetMoveSpeed(5f);
            }
        }
    }
}
