using UnityEngine;

namespace Pokiwar.Core
{
    /// <summary>
    /// Manages speed boost with stamina drain, cooldown, and visual trail feedback.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class SpeedBoostController : MonoBehaviour
    {
        [Header("Boost Settings")]
        [SerializeField] private float boostMultiplier = 1.8f;
        [SerializeField] private float maxStamina = 100f;
        [SerializeField] private float staminaDrainRate = 30f;
        [SerializeField] private float staminaRegenRate = 20f;
        [SerializeField] private float cooldownDuration = 2f;

        [Header("Visual")]
        [SerializeField] private TrailRenderer trailRenderer;

        private PlayerController playerController;
        private float currentStamina;
        private bool isBoosting;
        private bool isOnCooldown;
        private float cooldownTimer;

        public float StaminaPercent => maxStamina > 0f ? currentStamina / maxStamina : 0f;
        public bool IsBoosting => isBoosting;
        public bool IsOnCooldown => isOnCooldown;
        public float CooldownRemaining => isOnCooldown ? cooldownTimer : 0f;

        private void Awake()
        {
            playerController = GetComponent<PlayerController>();
            currentStamina = maxStamina;
        }

        private void Update()
        {
            HandleCooldown();
            HandleStamina();
            UpdateTrail();
        }

        private void HandleCooldown()
        {
            if (!isOnCooldown) return;

            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                isOnCooldown = false;
                cooldownTimer = 0f;
            }
        }

        private void HandleStamina()
        {
            if (isBoosting)
            {
                currentStamina -= staminaDrainRate * Time.deltaTime;
                if (currentStamina <= 0f)
                {
                    currentStamina = 0f;
                    StopBoost();
                    StartCooldown();
                }
            }
            else if (!isOnCooldown)
            {
                currentStamina = Mathf.Min(currentStamina + staminaRegenRate * Time.deltaTime, maxStamina);
            }
        }

        private void UpdateTrail()
        {
            if (trailRenderer != null)
                trailRenderer.emitting = isBoosting;
        }

        public void StartBoost()
        {
            if (isOnCooldown || currentStamina <= 0f || isBoosting) return;

            isBoosting = true;
            if (playerController != null)
                playerController.SetMoveSpeed(playerController.BaseSpeed * boostMultiplier);
        }

        public void StopBoost()
        {
            if (!isBoosting) return;

            isBoosting = false;
            if (playerController != null)
                playerController.ResetMoveSpeed();
        }

        private void StartCooldown()
        {
            isOnCooldown = true;
            cooldownTimer = cooldownDuration;
        }

        public void SetBoostMultiplier(float multiplier)
        {
            boostMultiplier = Mathf.Max(1f, multiplier);
        }

        public void RefillStamina()
        {
            currentStamina = maxStamina;
            isOnCooldown = false;
            cooldownTimer = 0f;
        }
    }
}
