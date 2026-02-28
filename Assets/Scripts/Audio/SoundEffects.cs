using UnityEngine;

namespace Pokiwar.Audio
{
    /// <summary>
    /// Centralized sound effect trigger system. Plays named sound events via AudioManager.
    /// </summary>
    public class SoundEffects : MonoBehaviour
    {
        [Header("Sound Clips")]
        [SerializeField] private AudioClip foodCollectSound;
        [SerializeField] private AudioClip levelUpSound;
        [SerializeField] private AudioClip evolutionSound;
        [SerializeField] private AudioClip combatHitSound;
        [SerializeField] private AudioClip playerDeathSound;
        [SerializeField] private AudioClip boostActivateSound;
        [SerializeField] private AudioClip dashAbilitySound;
        [SerializeField] private AudioClip areaAttackSound;
        [SerializeField] private AudioClip shieldAbilitySound;
        [SerializeField] private AudioClip uiButtonClickSound;

        public static SoundEffects Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void PlayFoodCollect(Vector3 position = default)
            => Play(foodCollectSound, position);

        public void PlayLevelUp()
            => Play(levelUpSound);

        public void PlayEvolution()
            => Play(evolutionSound);

        public void PlayCombatHit(Vector3 position = default)
            => Play(combatHitSound, position);

        public void PlayPlayerDeath(Vector3 position = default)
            => Play(playerDeathSound, position);

        public void PlayBoostActivate()
            => Play(boostActivateSound);

        public void PlayDashAbility()
            => Play(dashAbilitySound);

        public void PlayAreaAttack(Vector3 position = default)
            => Play(areaAttackSound, position);

        public void PlayShieldAbility()
            => Play(shieldAbilitySound);

        public void PlayUIButtonClick()
            => Play(uiButtonClickSound);

        private void Play(AudioClip clip, Vector3 position = default, float volumeScale = 1f)
        {
            if (clip == null) return;

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(clip, position, volumeScale);
            else
                AudioSource.PlayClipAtPoint(clip, position, volumeScale);
        }
    }
}
