using UnityEngine;
using Pokiwar.Core;

namespace Pokiwar.Evolution
{
    /// <summary>
    /// Food pickup behavior - grants XP when collected.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class FoodItem : MonoBehaviour
    {
        [SerializeField] private float xpValue = 1f;

        private SpriteRenderer spriteRenderer;
        private Collider2D col;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            col = GetComponent<Collider2D>();

            if (col != null)
                col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                EvolutionManager evolution = other.GetComponent<EvolutionManager>();
                if (evolution != null)
                {
                    evolution.AddXP(xpValue);
                    CollectFood(other.transform);
                }
            }
        }

        private void CollectFood(Transform collector)
        {
            Destroy(gameObject);
        }

        public void SetXPValue(float value)
        {
            xpValue = value;
        }

        public float GetXPValue() => xpValue;
    }
}
