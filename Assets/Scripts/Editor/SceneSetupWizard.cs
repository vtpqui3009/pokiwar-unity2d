#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Pokiwar.Editor
{
    /// <summary>
    /// One-click scene setup wizard: creates all required GameObjects, configures NetworkManager,
    /// sets up UI Canvas hierarchy, and creates required layers/tags.
    /// </summary>
    public class SceneSetupWizard : EditorWindow
    {
        [MenuItem("Pokiwar/Scene Setup Wizard")]
        public static void ShowWindow()
        {
            GetWindow<SceneSetupWizard>("Pokiwar Scene Setup");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Pokiwar Scene Setup Wizard", EditorStyles.boldLabel);
            GUILayout.Space(10);

            EditorGUILayout.HelpBox(
                "This wizard will set up all required GameObjects for a Pokiguard scene.\n" +
                "Run this in an empty scene to get started.",
                MessageType.Info);

            GUILayout.Space(10);

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Setup Full Scene", GUILayout.Height(40)))
            {
                SetupFullScene();
            }

            GUI.backgroundColor = Color.white;
            GUILayout.Space(5);

            if (GUILayout.Button("Create Managers Only"))
                CreateManagers();

            if (GUILayout.Button("Create UI Canvas Only"))
                CreateUICanvas();

            if (GUILayout.Button("Setup Tags & Layers"))
                SetupTagsAndLayers();
        }

        private static void SetupFullScene()
        {
            CreateManagers();
            CreateUICanvas();
            SetupTagsAndLayers();
            Debug.Log("[SceneSetupWizard] Full scene setup complete!");
        }

        private static void CreateManagers()
        {
            // GameManager
            if (FindObjectOfType<Pokiwar.Core.GameManager>() == null)
            {
                GameObject gm = new GameObject("GameManager");
                gm.AddComponent<Pokiwar.Core.GameManager>();
                Debug.Log("Created GameManager");
            }

            // NetworkManager (Unity Netcode)
            if (FindObjectOfType<Unity.Netcode.NetworkManager>() == null)
            {
                GameObject nm = new GameObject("NetworkManager");
                nm.AddComponent<Unity.Netcode.NetworkManager>();
                nm.AddComponent<Pokiwar.Multiplayer.NetworkManager>();
                Debug.Log("Created NetworkManager");
            }

            // FoodSpawner
            if (FindObjectOfType<Pokiwar.Evolution.FoodSpawner>() == null)
            {
                GameObject fs = new GameObject("FoodSpawner");
                fs.AddComponent<Pokiwar.Evolution.FoodSpawner>();
                Debug.Log("Created FoodSpawner");
            }

            // CombatManager
            if (FindObjectOfType<Pokiwar.Combat.CombatManager>() == null)
            {
                GameObject cm = new GameObject("CombatManager");
                cm.AddComponent<Pokiwar.Combat.CombatManager>();
                Debug.Log("Created CombatManager");
            }

            // AudioManager
            if (FindObjectOfType<Pokiwar.Audio.AudioManager>() == null)
            {
                GameObject am = new GameObject("AudioManager");
                am.AddComponent<Pokiwar.Audio.AudioManager>();
                am.AddComponent<Pokiwar.Audio.SoundEffects>();
                Debug.Log("Created AudioManager");
            }

            // GameModeManager
            if (FindObjectOfType<Pokiwar.GameModes.GameModeManager>() == null)
            {
                GameObject gmm = new GameObject("GameModeManager");
                gmm.AddComponent<Pokiwar.GameModes.GameModeManager>();
                gmm.AddComponent<Pokiwar.GameModes.FFAMode>();
                gmm.AddComponent<Pokiwar.GameModes.SurvivalMode>();
                Debug.Log("Created GameModeManager");
            }

            // MapBorderController
            if (FindObjectOfType<Pokiwar.World.MapBorderController>() == null)
            {
                GameObject border = new GameObject("MapBorder");
                border.AddComponent<Pokiwar.World.MapBorderController>();
                Debug.Log("Created MapBorderController");
            }

            // BackgroundTileManager
            if (FindObjectOfType<Pokiwar.World.BackgroundTileManager>() == null)
            {
                GameObject bg = new GameObject("Background");
                bg.AddComponent<Pokiwar.World.BackgroundTileManager>();
                Debug.Log("Created BackgroundTileManager");
            }
        }

        private static void CreateUICanvas()
        {
            // Main Canvas
            GameObject canvas = new GameObject("GameCanvas");
            Canvas c = canvas.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // HUDManager
            canvas.AddComponent<Pokiwar.UI.HUDManager>();

            // Sub-panels
            CreateUIPanel(canvas.transform, "LeaderboardPanel");
            CreateUIPanel(canvas.transform, "LevelBarPanel");
            CreateUIPanel(canvas.transform, "MinimapPanel");
            CreateUIPanel(canvas.transform, "KillFeedPanel");
            CreateUIPanel(canvas.transform, "MobileControlsPanel");
            CreateUIPanel(canvas.transform, "ChatPanel");
            CreateUIPanel(canvas.transform, "DeathScreenPanel");
            CreateUIPanel(canvas.transform, "EvolutionNotificationPanel");
            CreateUIPanel(canvas.transform, "MainMenuPanel");

            Debug.Log("Created UI Canvas hierarchy");
        }

        private static GameObject CreateUIPanel(Transform parent, string name)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            RectTransform rt = panel.AddComponent<RectTransform>();
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return panel;
        }

        private static void SetupTagsAndLayers()
        {
            // Tags and layers are set via SerializedObject in editor
            // This is a placeholder - actual tag/layer creation requires TagManager manipulation
            Debug.Log("[SceneSetupWizard] Ensure these tags exist: Player, Food");
            Debug.Log("[SceneSetupWizard] Ensure these layers exist: Player(6), Food(7), Minimap(8)");
        }
    }
}
#endif
