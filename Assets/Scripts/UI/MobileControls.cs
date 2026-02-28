using UnityEngine;
using UnityEngine.UI;
using Pokiwar.Core;
using Pokiwar.Combat;

namespace Pokiwar.UI
{
    /// <summary>
    /// Mobile touch controls: auto-repositioning joystick, attack, ability, and boost buttons.
    /// </summary>
    public class MobileControls : MonoBehaviour
    {
        [Header("Virtual Joystick")]
        [SerializeField] private Image joystickBackground;
        [SerializeField] private Image joystickHandle;
        [SerializeField] private float joystickRange = 50f;
        [SerializeField] private bool autoRepositionJoystick = true;

        [Header("Action Buttons")]
        [SerializeField] private Button boostButton;
        [SerializeField] private Button attackButton;
        [SerializeField] private Button abilityButton;

        [Header("Cooldown Visuals")]
        [SerializeField] private Image boostCooldownFill;
        [SerializeField] private Image abilityCooldownFill;

        private Vector2 joystickCenter;
        private bool isDragging;
        private PlayerController localPlayer;
        private SpeedBoostController boostController;
        private AbilitySystem abilitySystem;
        private Vector2 moveInput;

        private void Start()
        {
            if (joystickBackground != null)
                joystickCenter = joystickBackground.rectTransform.anchoredPosition;

            if (boostButton != null)
                boostButton.onClick.AddListener(OnBoostPressed);

            if (attackButton != null)
                attackButton.onClick.AddListener(OnAttackPressed);

            if (abilityButton != null)
                abilityButton.onClick.AddListener(OnAbilityPressed);
        }

        private void Update()
        {
            FindLocalPlayer();
            ApplyInput();
            UpdateCooldownVisuals();
        }

        private void FindLocalPlayer()
        {
            if (localPlayer != null) return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                localPlayer = player.GetComponent<PlayerController>();
                boostController = player.GetComponent<SpeedBoostController>();
                abilitySystem = player.GetComponent<AbilitySystem>();
            }
        }

        private void ApplyInput()
        {
            if (localPlayer != null && moveInput != Vector2.zero)
                localPlayer.SetInput(moveInput);
        }

        private void UpdateCooldownVisuals()
        {
            if (boostCooldownFill != null && boostController != null)
                boostCooldownFill.fillAmount = boostController.IsOnCooldown
                    ? boostController.CooldownRemaining / 2f
                    : 0f;

            if (abilityCooldownFill != null && abilitySystem != null)
                abilityCooldownFill.fillAmount = abilitySystem.DashCooldownPercent;
        }

        public void OnJoystickPointerDown(Vector2 screenPosition)
        {
            if (autoRepositionJoystick && joystickBackground != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    joystickBackground.rectTransform.parent as RectTransform,
                    screenPosition, null, out Vector2 localPoint);

                joystickBackground.rectTransform.anchoredPosition = localPoint;
                joystickCenter = localPoint;
            }
        }

        public void OnJoystickDrag(Vector2 position)
        {
            if (joystickHandle == null) return;

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
                joystickHandle.rectTransform.anchoredPosition = joystickCenter;

            moveInput = Vector2.zero;
            localPlayer?.ClearInput();
        }

        private void OnBoostPressed()
        {
            boostController?.StartBoost();
        }

        private void OnAttackPressed()
        {
            abilitySystem?.UseAbility(AbilityType.AreaAttack);
        }

        private void OnAbilityPressed()
        {
            abilitySystem?.UseAbility(AbilityType.Dash);
        }
    }
}
