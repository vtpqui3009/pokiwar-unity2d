#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Pokiwar.Editor
{
    /// <summary>
    /// Creates the Player prefab with all required components including new Pokiguard features.
    /// </summary>
    public class PlayerPrefabCreator : EditorWindow
    {
        [MenuItem("Pokiwar/Create Player Prefab")]
        public static void CreatePlayerPrefab()
        {
            string prefabPath = "Assets/Prefabs/Player.prefab";
            string prefabFolder = "Assets/Prefabs";

            if (!Directory.Exists(prefabFolder))
            {
                Directory.CreateDirectory(prefabFolder);
                AssetDatabase.Refresh();
            }

            GameObject player = new GameObject("Player");
            player.tag = "Player";
            player.layer = 6;

            // Physics
            Rigidbody2D rb = player.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 5f;
            rb.freezeRotation = true;

            CircleCollider2D collider = player.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;

            // Rendering
            SpriteRenderer sr = player.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 10;

            // Trail for boost
            TrailRenderer trail = player.AddComponent<TrailRenderer>();
            trail.time = 0.2f;
            trail.startWidth = 0.3f;
            trail.endWidth = 0f;
            trail.emitting = false;

            // Audio
            player.AddComponent<AudioSource>();

            // Core components
            player.AddComponent<Pokiwar.Core.PlayerController>();
            player.AddComponent<Pokiwar.Core.SpeedBoostController>();

            // Evolution components
            player.AddComponent<Pokiwar.Evolution.EvolutionManager>();
            player.AddComponent<Pokiwar.Evolution.EvolutionEffectController>();
            player.AddComponent<Pokiwar.Evolution.PetAnimationController>();

            // Combat components
            player.AddComponent<Pokiwar.Combat.HealthController>();
            player.AddComponent<Pokiwar.Combat.AbilitySystem>();

            // Multiplayer components
            player.AddComponent<Pokiwar.Multiplayer.PlayerNetwork>();
            player.AddComponent<Pokiwar.Multiplayer.PlayerSync>();
            player.AddComponent<Pokiwar.Multiplayer.SpectatorMode>();

            // Save prefab
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(player, prefabPath);
            DestroyImmediate(player);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Player prefab created at: " + prefabPath);
            Selection.activeObject = prefab;
        }

        [MenuItem("Pokiwar/Create Food Prefab")]
        public static void CreateFoodPrefab()
        {
            string prefabPath = "Assets/Prefabs/Food.prefab";
            string prefabFolder = "Assets/Prefabs";

            if (!Directory.Exists(prefabFolder))
            {
                Directory.CreateDirectory(prefabFolder);
                AssetDatabase.Refresh();
            }

            GameObject food = new GameObject("Food");
            food.tag = "Food";
            food.layer = 7;

            CircleCollider2D collider = food.AddComponent<CircleCollider2D>();
            collider.isTrigger = true;
            collider.radius = 0.3f;

            Rigidbody2D rb = food.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 5f;

            SpriteRenderer sr = food.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 1;

            food.AddComponent<Pokiwar.Evolution.FoodItem>();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(food, prefabPath);
            DestroyImmediate(food);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("Food prefab created at: " + prefabPath);
            Selection.activeObject = prefab;
        }
    }
}
#endif
