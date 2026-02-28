using UnityEngine;
using Pokiwar.Core;

namespace Pokiwar.Evolution
{
    /// <summary>
    /// Spawns food items across the map at regular intervals.
    /// </summary>
    public class FoodSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject foodPrefab;
        [SerializeField] private int maxFoodCount = 100;
        [SerializeField] private float spawnInterval = 0.5f;
        [SerializeField] private float minDistanceFromPlayer = 5f;

        [Header("Food Types")]
        [SerializeField] private Sprite[] foodSprites;
        [SerializeField] private float[] foodXPValues = { 1f, 2f, 5f, 10f };

        private float spawnTimer;

        private void Update()
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                TrySpawnFood();
            }
        }

        private void TrySpawnFood()
        {
            GameObject[] food = GameObject.FindGameObjectsWithTag("Food");
            if (food.Length >= maxFoodCount)
                return;

            Vector2 spawnPosition = GetRandomSpawnPosition();
            SpawnFood(spawnPosition);
        }

        private Vector2 GetRandomSpawnPosition()
        {
            Vector2 position;
            int attempts = 0;
            const int maxAttempts = 10;

            do
            {
                position = GameManager.Instance.GetRandomPosition();
                attempts++;
            } while (IsTooCloseToPlayer(position) && attempts < maxAttempts);

            return position;
        }

        private bool IsTooCloseToPlayer(Vector2 position)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
                return false;

            return Vector2.Distance(position, player.transform.position) < minDistanceFromPlayer;
        }

        private void SpawnFood(Vector2 position)
        {
            if (foodPrefab == null)
            {
                CreateDefaultFood(position);
                return;
            }

            GameObject food = Instantiate(foodPrefab, position, Quaternion.identity);
            ConfigureFood(food);
        }

        private void CreateDefaultFood(Vector2 position)
        {
            GameObject food = new GameObject("Food");
            food.transform.position = position;
            food.tag = "Food";

            food.AddComponent<SpriteRenderer>();
            food.AddComponent<CircleCollider2D>();
            food.AddComponent<Rigidbody2D>();

            FoodItem foodItem = food.AddComponent<FoodItem>();
            foodItem.SetXPValue(RandomFoodValue());

            ConfigureFood(food);
        }

        private void ConfigureFood(GameObject food)
        {
            SpriteRenderer sr = food.GetComponent<SpriteRenderer>();
            CircleCollider2D collider = food.GetComponent<CircleCollider2D>();
            Rigidbody2D rb = food.GetComponent<Rigidbody2D>();

            if (sr != null && foodSprites != null && foodSprites.Length > 0)
            {
                sr.sprite = foodSprites[Random.Range(0, foodSprites.Length)];
                sr.sortingOrder = 1;
            }

            if (collider != null)
            {
                collider.isTrigger = true;
                collider.radius = 0.3f;
            }

            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.linearDamping = 5f;
            }
        }

        private float RandomFoodValue()
        {
            return foodXPValues[Random.Range(0, foodXPValues.Length)];
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            if (GameManager.Instance != null)
            {
                Gizmos.DrawWireCube(transform.position, new Vector3(GameManager.Instance.MapWidth, GameManager.Instance.MapHeight, 0));
            }
        }
    }
}
