using UnityEngine;
using UnityEngine.UI;
using Pokiwar.Core;
using Pokiwar.Evolution;

namespace Pokiwar.UI
{
    /// <summary>
    /// Displays minimap with player positions, food dots, and danger zone overlay.
    /// </summary>
    public class MinimapUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private RawImage minimapImage;
        [SerializeField] private GameObject playerIndicatorPrefab;
        [SerializeField] private GameObject foodIndicatorPrefab;
        [SerializeField] private Transform indicatorsContainer;

        [Header("Settings")]
        [SerializeField] private Color localPlayerColor = Color.green;
        [SerializeField] private Color enemyColor = Color.red;
        [SerializeField] private Color foodColor = Color.yellow;
        [SerializeField] private float playerIndicatorSize = 8f;
        [SerializeField] private float foodIndicatorSize = 4f;
        [SerializeField] private int maxFoodIndicators = 50;

        [Header("Zoom")]
        [SerializeField] private float minZoom = 0.5f;
        [SerializeField] private float maxZoom = 2f;
        [SerializeField] private float currentZoom = 1f;

        private GameObject[] playerIndicators = new GameObject[20];
        private GameObject[] foodIndicators;
        private Camera minimapCamera;
        private RenderTexture minimapRenderTexture;
        private GameObject localPlayer;

        private void Start()
        {
            if (GameManager.Instance == null) return;

            localPlayer = GameObject.FindGameObjectWithTag("Player");
            foodIndicators = new GameObject[maxFoodIndicators];

            SetupMinimapCamera();
            SetupPlayerIndicators();
            SetupFoodIndicators();
        }

        private void SetupMinimapCamera()
        {
            GameObject camObj = new GameObject("MinimapCamera");
            camObj.transform.SetParent(transform);
            minimapCamera = camObj.AddComponent<Camera>();

            float mapSize = Mathf.Max(GameManager.Instance.MapWidth, GameManager.Instance.MapHeight);
            minimapCamera.orthographic = true;
            minimapCamera.orthographicSize = mapSize / 2f;
            minimapCamera.transform.position = new Vector3(0, 0, -50);
            minimapCamera.cullingMask = LayerMask.GetMask("Minimap");
            minimapCamera.clearFlags = CameraClearFlags.SolidColor;
            minimapCamera.backgroundColor = new Color(0.15f, 0.15f, 0.15f, 1f);

            minimapRenderTexture = new RenderTexture(256, 256, 0);
            minimapCamera.targetTexture = minimapRenderTexture;

            if (minimapImage != null)
                minimapImage.texture = minimapRenderTexture;
        }

        private void SetupPlayerIndicators()
        {
            if (indicatorsContainer == null || playerIndicatorPrefab == null) return;

            for (int i = 0; i < playerIndicators.Length; i++)
            {
                playerIndicators[i] = Instantiate(playerIndicatorPrefab, indicatorsContainer);
                playerIndicators[i].SetActive(false);
            }
        }

        private void SetupFoodIndicators()
        {
            if (indicatorsContainer == null || foodIndicatorPrefab == null) return;

            for (int i = 0; i < foodIndicators.Length; i++)
            {
                foodIndicators[i] = Instantiate(foodIndicatorPrefab, indicatorsContainer);
                foodIndicators[i].SetActive(false);
            }
        }

        private void Update()
        {
            UpdatePlayerIndicators();
            UpdateFoodIndicators();
            HandleZoomInput();
        }

        private void UpdatePlayerIndicators()
        {
            if (GameManager.Instance == null) return;

            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            for (int i = 0; i < playerIndicators.Length; i++)
            {
                if (playerIndicators[i] == null) continue;

                if (i < players.Length)
                {
                    playerIndicators[i].SetActive(true);
                    PlaceIndicator(playerIndicators[i], players[i].transform.position, playerIndicatorSize);

                    Image img = playerIndicators[i].GetComponent<Image>();
                    if (img != null)
                        img.color = players[i] == localPlayer ? localPlayerColor : enemyColor;
                }
                else
                {
                    playerIndicators[i].SetActive(false);
                }
            }
        }

        private void UpdateFoodIndicators()
        {
            if (GameManager.Instance == null || foodIndicators == null) return;

            GameObject[] foodItems = GameObject.FindGameObjectsWithTag("Food");

            for (int i = 0; i < foodIndicators.Length; i++)
            {
                if (foodIndicators[i] == null) continue;

                if (i < foodItems.Length)
                {
                    foodIndicators[i].SetActive(true);
                    PlaceIndicator(foodIndicators[i], foodItems[i].transform.position, foodIndicatorSize);

                    Image img = foodIndicators[i].GetComponent<Image>();
                    if (img != null) img.color = foodColor;
                }
                else
                {
                    foodIndicators[i].SetActive(false);
                }
            }
        }

        private void PlaceIndicator(GameObject indicator, Vector2 worldPos, float size)
        {
            if (GameManager.Instance == null) return;

            float mapW = GameManager.Instance.MapWidth;
            float mapH = GameManager.Instance.MapHeight;

            float normX = (worldPos.x + mapW / 2f) / mapW;
            float normY = (worldPos.y + mapH / 2f) / mapH;

            RectTransform rect = indicator.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(normX, normY);
                rect.anchorMax = new Vector2(normX, normY);
                rect.sizeDelta = new Vector2(size, size);
            }
        }

        private void HandleZoomInput()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                currentZoom = Mathf.Clamp(currentZoom + scroll, minZoom, maxZoom);
                if (minimapCamera != null)
                {
                    float mapSize = Mathf.Max(GameManager.Instance.MapWidth, GameManager.Instance.MapHeight);
                    minimapCamera.orthographicSize = (mapSize / 2f) / currentZoom;
                }
            }
        }

        private void OnDestroy()
        {
            minimapRenderTexture?.Release();
        }
    }
}
