using System.Collections.Generic;
using UnityEngine;

namespace SmartScienceLab.PhysicsLab.ProjectileMotion
{
    [RequireComponent(typeof(Projectile))]
    public class ProjectileTrail : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject trailMarkerPrefab;
        [SerializeField] private GameObject apexMarkerPrefab;
        [SerializeField] private GameObject rangeMarkerPrefab;

        [Header("Settings")]
        [SerializeField] private float minDistanceBetweenMarkers = 0.15f;

        private Projectile projectile;
        private Vector3 lastMarkerPosition;
        private readonly List<GameObject> spawnedVisuals = new List<GameObject>();

        public IReadOnlyList<GameObject> SpawnedVisuals => spawnedVisuals;

        private void Awake()
        {
            projectile = GetComponent<Projectile>();
        }

        private void OnEnable()
        {
            if (projectile != null)
            {
                projectile.OnApexReached += HandleApexReached;
                projectile.OnLanded += HandleLanded;
            }
        }

        private void OnDisable()
        {
            if (projectile != null)
            {
                projectile.OnApexReached -= HandleApexReached;
                projectile.OnLanded -= HandleLanded;
            }
        }

        private void Update()
        {
            if (projectile == null || !projectile.IsLaunched || projectile.HasLanded) return;

            // Spawn trail markers at discrete spatial intervals
            if (trailMarkerPrefab != null)
            {
                float dist = Vector3.Distance(transform.position, lastMarkerPosition);
                if (dist >= minDistanceBetweenMarkers || spawnedVisuals.Count == 0)
                {
                    lastMarkerPosition = transform.position;
                    GameObject marker = Instantiate(trailMarkerPrefab, transform.position, Quaternion.identity);
                    spawnedVisuals.Add(marker);
                }
            }
        }

        private void HandleApexReached(float height, Vector3 apexPosition)
        {
            if (apexMarkerPrefab != null)
            {
                GameObject apexObj = Instantiate(apexMarkerPrefab, apexPosition + Vector3.up * 0.1f, Quaternion.identity);
                BillboardDisplay billboard = apexObj.GetComponent<BillboardDisplay>();
                if (billboard != null)
                {
                    billboard.SetText($"H_max = {height:F2} m");
                }
                spawnedVisuals.Add(apexObj);
            }
        }

        private void HandleLanded(Projectile p)
        {
            if (rangeMarkerPrefab != null)
            {
                GameObject rangeObj = Instantiate(rangeMarkerPrefab, transform.position + Vector3.up * 0.05f, Quaternion.identity);
                BillboardDisplay billboard = rangeObj.GetComponent<BillboardDisplay>();
                if (billboard != null)
                {
                    billboard.SetText($"Range = {p.MeasRange:F2} m");
                }
                spawnedVisuals.Add(rangeObj);
            }
        }

        public void ClearVisuals()
        {
            for (int i = 0; i < spawnedVisuals.Count; i++)
            {
                if (spawnedVisuals[i] != null)
                {
                    Destroy(spawnedVisuals[i]);
                }
            }
            spawnedVisuals.Clear();
        }

        private void OnDestroy()
        {
            ClearVisuals();
        }
    }
}
