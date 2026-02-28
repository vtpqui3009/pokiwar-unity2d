using UnityEngine;
using Pokiwar.Core;

namespace Pokiwar.Evolution
{
    /// <summary>
    /// Spawns food items across the map with weighted random types and cluster support.
    /// Spawn weights: 70% Normal, 20% Rare, 8% Mega, 2% Poison (Speed via rare events).
    /// </summary>
    public class FoodSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private GameObject foodPrefab;
        [SerializeField] private int maxFoodCount = 200;
        [SerializeField] private float spawnInterval = 0.3f;
        [SerializeField] private float minDistanceFromPlayer = 3f;

        [Header("Cluster Settings")]
        [SerializeField] private bool enableClusters = true;
        [SerializeField] private int clusterSize = 5;
        [SerializeField] private float clusterRadius = 3f;
        [SerializeField] private float clusterSpawnChance = 0.15f;

        [Header("Rare Food Events")]
        [SerializeField] private float megaFoodEventInterval = 30f;
        [SerializeField] private int megaFoodEventCount = 5;

        // Spawn weights (must sum to 100)
        private const float WeightNormal = 70f;
        private const float WeightRare   = 20f;
        private const float WeightMega   = 8f;
        private const float WeightPoison = 2f;

        private float spawnTimer;
        private float megaEventTimer;

        private void Update()
        {
            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                TrySpawnFood();
            }

            megaEventTimer += Time.deltaTime;
            if (megaEventTimer >= megaFoodEventInterval)
            {
                megaEventTimer = 0f;
                SpawnMegaFoodEvent();
            }
        }

        private void TrySpawnFood()
        {
            GameObject[] food = GameObject.FindGameObjectsWithTag("Food");
            if (food.Length >= maxFoodCount) return;

            if (enableClusters && Random.value < clusterSpawnChance)
            {
                SpawnCluster();
            }
            else
            {
                Vector2 spawnPos = GetRandomSpawnPosition();
                SpawnFoodAt(spawnPos, GetWeightedFoodType());
            }
        }

        private void SpawnCluster()
        {
            Vector2 center = GetRandomSpawnPosition();
            for (int i = 0; i < clusterSize; i++)
            {
                Vector2 offset = Random.insideUnitCircle * clusterRadius;
                Vector2 pos = center + offset;
                if (GameManager.Instance != null)
                    pos = GameManager.Instance.ClampToBounds(pos);

                SpawnFoodAt(pos, FoodType.Normal);
            }
        }

        private void SpawnMegaFoodEvent()
        {
            for (int i = 0; i < megaFoodEventCount; i++)
            {
                Vector2 pos = GetRandomSpawnPosition();
                SpawnFoodAt(pos, FoodType.Mega);
            }
        }

        private Vector2 GetRandomSpawnPosition()
        {
            if (GameManager.Instance == null)
                return Vector2.zero;

            Vector2 position;
            int attempts = 0;
            const int maxAttempts = 10;

            do
            {
                position = GameManager.Instance.GetRandomPosition();
                attempts++;
            } while (IsTooCloseToAnyPlayer(position) && attempts < maxAttempts);

            return position;
        }

        private bool IsTooCloseToAnyPlayer(Vector2 position)
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                if (Vector2.Distance(position, player.transform.position) < minDistanceFromPlayer)
                    return true;
            }
            return false;
        }

        private void SpawnFoodAt(Vector2 position, FoodType type)
        {
            GameObject food;

            if (foodPrefab != null)
            {
                food = Instantiate(foodPrefab, position, Quaternion.identity);
            }
            else
            {
                food = CreateDefaultFoodObject(position);
            }

            FoodItem foodItem = food.GetComponent<FoodItem>();
            if (foodItem != null)
                foodItem.SetFoodType(type);

            food.tag = "Food";
        }

        private GameObject CreateDefaultFoodObject(Vector2 position)
        {
            GameObject food = new GameObject("Food");
            food.transform.position = position;
            food.tag = "Food";

            SpriteRenderer sr = food.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 1;

            CircleCollider2D col = food.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.3f;

            Rigidbody2D rb = food.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 5f;

            food.AddComponent<FoodItem>();
            return food;
        }

        private FoodType GetWeightedFoodType()
        {
            float roll = Random.Range(0f, 100f);

            if (roll < WeightNormal)
                return FoodType.Normal;
            if (roll < WeightNormal + WeightRare)
                return FoodType.Rare;
            if (roll < WeightNormal + WeightRare + WeightMega)
                return FoodType.Mega;

            return FoodType.Poison;
        }

        private void OnDrawGizmosSelected()
        {
            if (GameManager.Instance == null) return;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(transform.position,
                new Vector3(GameManager.Instance.MapWidth, GameManager.Instance.MapHeight, 0));
        }
    }
}
