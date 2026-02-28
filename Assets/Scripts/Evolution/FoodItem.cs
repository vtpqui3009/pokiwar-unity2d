using UnityEngine;
using Pokiwar.Core;

namespace Pokiwar.Evolution
{
    /// <summary>
    /// Food type determines XP value and visual appearance.
    /// </summary>
    public enum FoodType
    {
        Normal,   // 1 XP  - white dot
        Rare,     // 5 XP  - yellow star
        Mega,     // 25 XP - rainbow
        Poison,   // -5 XP - purple (danger)
        Speed     // 0 XP  - blue (speed boost)
    }

    /// <summary>
    /// Food pickup behavior - grants XP (or applies effects) when collected.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class FoodItem : MonoBehaviour
    {
        [SerializeField] private FoodType foodType = FoodType.Normal;
        [SerializeField] private float xpValue = 1f;
        [SerializeField] private float speedBoostDuration = 3f;

        private SpriteRenderer spriteRenderer;
        private Collider2D col;

        public FoodType Type => foodType;
        public float XPValue => xpValue;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            col = GetComponent<Collider2D>();

            if (col != null)
                col.isTrigger = true;

            ApplyVisuals();
        }

        private void ApplyVisuals()
        {
            if (spriteRenderer == null) return;

            Color foodColor = foodType switch
            {
                FoodType.Normal  => Color.white,
                FoodType.Rare    => Color.yellow,
                FoodType.Mega    => new Color(1f, 0.5f, 0f), // orange-ish for rainbow placeholder
                FoodType.Poison  => new Color(0.6f, 0f, 0.8f),
                FoodType.Speed   => new Color(0.2f, 0.6f, 1f),
                _                => Color.white
            };

            spriteRenderer.color = foodColor;

            // Scale by type
            float scale = foodType switch
            {
                FoodType.Normal  => 0.3f,
                FoodType.Rare    => 0.45f,
                FoodType.Mega    => 0.65f,
                FoodType.Poison  => 0.4f,
                FoodType.Speed   => 0.4f,
                _                => 0.3f
            };

            transform.localScale = Vector3.one * scale;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player")) return;

            EvolutionManager evolution = other.GetComponent<EvolutionManager>();
            if (evolution == null) return;

            ApplyFoodEffect(other.gameObject, evolution);
            evolution.GetPlayerData()?.RecordFoodCollected();
            Destroy(gameObject);
        }

        private void ApplyFoodEffect(GameObject player, EvolutionManager evolution)
        {
            switch (foodType)
            {
                case FoodType.Normal:
                case FoodType.Rare:
                case FoodType.Mega:
                    evolution.AddXP(xpValue);
                    break;

                case FoodType.Poison:
                    evolution.AddXP(-Mathf.Abs(xpValue));
                    break;

                case FoodType.Speed:
                    SpeedBoostController boost = player.GetComponent<SpeedBoostController>();
                    if (boost != null)
                        boost.StartBoost();
                    break;
            }
        }

        public void SetFoodType(FoodType type)
        {
            foodType = type;
            xpValue = type switch
            {
                FoodType.Normal  => 1f,
                FoodType.Rare    => 5f,
                FoodType.Mega    => 25f,
                FoodType.Poison  => 5f,
                FoodType.Speed   => 0f,
                _                => 1f
            };
            ApplyVisuals();
        }

        public void SetXPValue(float value)
        {
            xpValue = value;
        }

        public float GetXPValue() => xpValue;
    }
}
