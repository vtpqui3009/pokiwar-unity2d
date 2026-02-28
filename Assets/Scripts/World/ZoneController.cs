using UnityEngine;
using System.Collections;
using Pokiwar.Core;
using Pokiwar.Evolution;

namespace Pokiwar.World
{
    /// <summary>
    /// Zone types that affect gameplay.
    /// </summary>
    public enum ZoneType
    {
        XPBoost,   // 2x XP from food
        Speed,     // +20% movement speed
        Danger,    // Increased player damage
        Safe       // No PvP
    }

    /// <summary>
    /// Controls gameplay zones that rotate every 60 seconds.
    /// </summary>
    public class ZoneController : MonoBehaviour
    {
        [Header("Zone Settings")]
        [SerializeField] private float zoneRadius = 15f;
        [SerializeField] private float rotationInterval = 60f;
        [SerializeField] private ZoneType currentZoneType = ZoneType.XPBoost;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer zoneVisual;
        [SerializeField] private float visualAlpha = 0.3f;

        private float rotationTimer;

        public ZoneType CurrentZone => currentZoneType;
        public float ZoneRadius => zoneRadius;

        private void Start()
        {
            UpdateZoneVisual();
        }

        private void Update()
        {
            rotationTimer += Time.deltaTime;
            if (rotationTimer >= rotationInterval)
            {
                rotationTimer = 0f;
                RotateZone();
            }

            ApplyZoneEffects();
        }

        private void RotateZone()
        {
            int nextZone = ((int)currentZoneType + 1) % System.Enum.GetValues(typeof(ZoneType)).Length;
            currentZoneType = (ZoneType)nextZone;
            UpdateZoneVisual();
        }

        private void UpdateZoneVisual()
        {
            if (zoneVisual == null) return;

            Color zoneColor = currentZoneType switch
            {
                ZoneType.XPBoost => new Color(0f, 1f, 0f, visualAlpha),
                ZoneType.Speed   => new Color(0f, 0.5f, 1f, visualAlpha),
                ZoneType.Danger  => new Color(1f, 0f, 0f, visualAlpha),
                ZoneType.Safe    => new Color(1f, 1f, 1f, visualAlpha),
                _                => new Color(0.5f, 0.5f, 0.5f, visualAlpha)
            };

            zoneVisual.color = zoneColor;
        }

        private void ApplyZoneEffects()
        {
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                if (!IsInZone(player.transform.position)) continue;

                switch (currentZoneType)
                {
                    case ZoneType.Speed:
                        PlayerController pc = player.GetComponent<PlayerController>();
                        // Speed boost is applied as a multiplier - handled by SpeedBoostController
                        break;
                }
            }
        }

        public bool IsInZone(Vector2 position)
        {
            return Vector2.Distance(position, transform.position) <= zoneRadius;
        }

        public float GetXPMultiplier(Vector2 position)
        {
            if (!IsInZone(position)) return 1f;
            return currentZoneType == ZoneType.XPBoost ? 2f : 1f;
        }

        public bool IsSafeZone(Vector2 position)
        {
            return IsInZone(position) && currentZoneType == ZoneType.Safe;
        }

        public bool IsDangerZone(Vector2 position)
        {
            return IsInZone(position) && currentZoneType == ZoneType.Danger;
        }

        private void OnDrawGizmosSelected()
        {
            Color gizmoColor = currentZoneType switch
            {
                ZoneType.XPBoost => Color.green,
                ZoneType.Speed   => Color.blue,
                ZoneType.Danger  => Color.red,
                ZoneType.Safe    => Color.white,
                _                => Color.gray
            };

            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
            Gizmos.DrawSphere(transform.position, zoneRadius);
        }
    }
}
