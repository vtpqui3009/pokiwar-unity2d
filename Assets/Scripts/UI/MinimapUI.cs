using UnityEngine;
using UnityEngine.UI;
using Pokiwar.Core;

namespace Pokiwar.UI
{
    /// <summary>
    /// Displays minimap showing player positions relative to map bounds.
    /// </summary>
    public class MinimapUI : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private RawImage minimapImage;
        [SerializeField] private GameObject playerIndicatorPrefab;
        [SerializeField] private Transform indicatorsContainer;

        [Header("Settings")]
        [SerializeField] private Color playerColor = Color.green;
        [SerializeField] private Color enemyColor = Color.red;
        [SerializeField] private float indicatorSize = 5f;

        private GameObject[] playerIndicators = new GameObject[20];
        private Camera minimapCamera;
        private RenderTexture minimapRenderTexture;

        private void Start()
        {
            if (GameManager.Instance == null)
                return;

            SetupMinimapCamera();
            SetupPlayerIndicators();
        }

        private void SetupMinimapCamera()
        {
            GameObject camObj = new GameObject("MinimapCamera");
            camObj.transform.SetParent(transform);
            minimapCamera = camObj.AddComponent<Camera>();

            minimapCamera.orthographic = true;
            minimapCamera.orthographicSize = Mathf.Max(GameManager.Instance.MapWidth, GameManager.Instance.MapHeight) / 2f;
            minimapCamera.transform.position = new Vector3(0, 0, -50);
            minimapCamera.cullingMask = LayerMask.GetMask("Minimap");
            minimapCamera.clearFlags = CameraClearFlags.SolidColor;
            minimapCamera.backgroundColor = new Color(0.2f, 0.2f, 0.2f, 1f);

            minimapRenderTexture = new RenderTexture(256, 256, 0);
            minimapCamera.targetTexture = minimapRenderTexture;

            if (minimapImage != null)
                minimapImage.texture = minimapRenderTexture;
        }

        private void SetupPlayerIndicators()
        {
            if (indicatorsContainer == null || playerIndicatorPrefab == null)
                return;

            for (int i = 0; i < playerIndicators.Length; i++)
            {
                playerIndicators[i] = Instantiate(playerIndicatorPrefab, indicatorsContainer);
                playerIndicators[i].SetActive(false);
            }
        }

        private void Update()
        {
            UpdatePlayerIndicators();
        }

        private void UpdatePlayerIndicators()
        {
            if (GameManager.Instance == null)
                return;

            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

            for (int i = 0; i < playerIndicators.Length; i++)
            {
                if (i < players.Length)
                {
                    playerIndicators[i].SetActive(true);

                    Vector2 playerPos = players[i].transform.position;
                    Vector2 mapSize = new Vector2(GameManager.Instance.MapWidth, GameManager.Instance.MapHeight);

                    float normX = (playerPos.x + mapSize.x / 2f) / mapSize.x;
                    float normY = (playerPos.y + mapSize.y / 2f) / mapSize.y;

                    RectTransform rect = playerIndicators[i].GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.anchorMin = new Vector2(normX, normY);
                        rect.anchorMax = new Vector2(normX, normY);
                        rect.sizeDelta = new Vector2(indicatorSize, indicatorSize);
                    }
                }
                else
                {
                    playerIndicators[i].SetActive(false);
                }
            }
        }

        private void OnDestroy()
        {
            if (minimapRenderTexture != null)
            {
                minimapRenderTexture.Release();
            }
        }
    }
}
