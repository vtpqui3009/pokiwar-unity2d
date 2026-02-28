using UnityEngine;
using System.Collections;
using Pokiwar.Core;
using Pokiwar.Combat;

namespace Pokiwar.World
{
    /// <summary>
    /// Controls map border with visual boundary, damage outside bounds, and shrinking safe zone for Survival mode.
    /// </summary>
    public class MapBorderController : MonoBehaviour
    {
        [Header("Border Settings")]
        [SerializeField] private float borderDamagePerSecond = 10f;
        [SerializeField] private float warningDistance = 10f;

        [Header("Survival Mode - Shrinking Zone")]
        [SerializeField] private bool enableShrinkingZone = false;
        [SerializeField] private float shrinkStartDelay = 60f;
        [SerializeField] private float shrinkRate = 0.5f;
        [SerializeField] private float minZoneSize = 20f;

        [Header("Visual")]
        [SerializeField] private LineRenderer borderRenderer;
        [SerializeField] private Color safeColor = Color.green;
        [SerializeField] private Color dangerColor = Color.red;

        private float currentZoneWidth;
        private float currentZoneHeight;
        private bool isShrinking;
        private float shrinkTimer;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                currentZoneWidth = GameManager.Instance.MapWidth;
                currentZoneHeight = GameManager.Instance.MapHeight;
            }

            DrawBorder();

            if (enableShrinkingZone)
                StartCoroutine(ShrinkZoneRoutine());
        }

        private void Update()
        {
            if (isShrinking)
            {
                currentZoneWidth = Mathf.Max(minZoneSize, currentZoneWidth - shrinkRate * Time.deltaTime);
                currentZoneHeight = Mathf.Max(minZoneSize, currentZoneHeight - shrinkRate * Time.deltaTime);
                DrawBorder();
            }

            DamagePlayersOutsideBounds();
        }

        private void DamagePlayersOutsideBounds()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                Vector2 pos = player.transform.position;
                if (!IsInsideZone(pos))
                {
                    HealthController hc = player.GetComponent<HealthController>();
                    hc?.TakeDamage(borderDamagePerSecond * Time.deltaTime, null);
                }
            }
        }

        private bool IsInsideZone(Vector2 position)
        {
            float halfW = currentZoneWidth / 2f;
            float halfH = currentZoneHeight / 2f;
            return position.x >= -halfW && position.x <= halfW &&
                   position.y >= -halfH && position.y <= halfH;
        }

        private void DrawBorder()
        {
            if (borderRenderer == null) return;

            float halfW = currentZoneWidth / 2f;
            float halfH = currentZoneHeight / 2f;

            borderRenderer.positionCount = 5;
            borderRenderer.SetPosition(0, new Vector3(-halfW, -halfH, 0));
            borderRenderer.SetPosition(1, new Vector3(halfW, -halfH, 0));
            borderRenderer.SetPosition(2, new Vector3(halfW, halfH, 0));
            borderRenderer.SetPosition(3, new Vector3(-halfW, halfH, 0));
            borderRenderer.SetPosition(4, new Vector3(-halfW, -halfH, 0));

            borderRenderer.startColor = isShrinking ? dangerColor : safeColor;
            borderRenderer.endColor = isShrinking ? dangerColor : safeColor;
        }

        private IEnumerator ShrinkZoneRoutine()
        {
            yield return new WaitForSeconds(shrinkStartDelay);
            isShrinking = true;
        }

        public bool IsNearBorder(Vector2 position)
        {
            float halfW = currentZoneWidth / 2f;
            float halfH = currentZoneHeight / 2f;
            return position.x < -halfW + warningDistance || position.x > halfW - warningDistance ||
                   position.y < -halfH + warningDistance || position.y > halfH - warningDistance;
        }

        public float GetCurrentZoneWidth() => currentZoneWidth;
        public float GetCurrentZoneHeight() => currentZoneHeight;
    }
}
