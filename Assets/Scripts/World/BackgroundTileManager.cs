using UnityEngine;
using Pokiwar.Core;

namespace Pokiwar.World
{
    /// <summary>
    /// Manages tiled background with grid lines, parallax scrolling, and zone color overlays.
    /// Uses object pooling for efficient tile management.
    /// </summary>
    public class BackgroundTileManager : MonoBehaviour
    {
        [Header("Tile Settings")]
        [SerializeField] private GameObject tilePrefab;
        [SerializeField] private float tileSize = 10f;
        [SerializeField] private int tilesX = 12;
        [SerializeField] private int tilesY = 12;

        [Header("Grid Lines")]
        [SerializeField] private bool showGridLines = true;
        [SerializeField] private Color gridLineColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

        [Header("Parallax")]
        [SerializeField] private float parallaxFactor = 0.1f;

        private GameObject[] tilePool;
        private Camera mainCamera;
        private Vector3 lastCameraPos;

        private void Start()
        {
            mainCamera = Camera.main;
            if (mainCamera != null)
                lastCameraPos = mainCamera.transform.position;

            InitializeTilePool();
        }

        private void InitializeTilePool()
        {
            int totalTiles = tilesX * tilesY;
            tilePool = new GameObject[totalTiles];

            for (int i = 0; i < totalTiles; i++)
            {
                if (tilePrefab != null)
                {
                    tilePool[i] = Instantiate(tilePrefab, transform);
                }
                else
                {
                    tilePool[i] = CreateDefaultTile();
                }
            }

            LayoutTiles();
        }

        private GameObject CreateDefaultTile()
        {
            GameObject tile = new GameObject("Tile");
            tile.transform.SetParent(transform);

            SpriteRenderer sr = tile.AddComponent<SpriteRenderer>();
            sr.color = new Color(0.2f, 0.25f, 0.2f, 1f);
            sr.sortingOrder = -10;

            return tile;
        }

        private void LayoutTiles()
        {
            float startX = -(tilesX / 2f) * tileSize;
            float startY = -(tilesY / 2f) * tileSize;

            for (int y = 0; y < tilesY; y++)
            {
                for (int x = 0; x < tilesX; x++)
                {
                    int index = y * tilesX + x;
                    if (index >= tilePool.Length) break;

                    float posX = startX + x * tileSize + tileSize / 2f;
                    float posY = startY + y * tileSize + tileSize / 2f;

                    tilePool[index].transform.localPosition = new Vector3(posX, posY, 0f);
                    tilePool[index].transform.localScale = Vector3.one * tileSize;
                }
            }
        }

        private void Update()
        {
            if (mainCamera == null) return;

            ApplyParallax();
        }

        private void ApplyParallax()
        {
            Vector3 cameraDelta = mainCamera.transform.position - lastCameraPos;
            transform.position += cameraDelta * parallaxFactor;
            lastCameraPos = mainCamera.transform.position;
        }

        private void OnDrawGizmosSelected()
        {
            if (!showGridLines) return;

            Gizmos.color = gridLineColor;
            float totalW = tilesX * tileSize;
            float totalH = tilesY * tileSize;
            float startX = transform.position.x - totalW / 2f;
            float startY = transform.position.y - totalH / 2f;

            for (int x = 0; x <= tilesX; x++)
            {
                float xPos = startX + x * tileSize;
                Gizmos.DrawLine(new Vector3(xPos, startY, 0), new Vector3(xPos, startY + totalH, 0));
            }

            for (int y = 0; y <= tilesY; y++)
            {
                float yPos = startY + y * tileSize;
                Gizmos.DrawLine(new Vector3(startX, yPos, 0), new Vector3(startX + totalW, yPos, 0));
            }
        }
    }
}
