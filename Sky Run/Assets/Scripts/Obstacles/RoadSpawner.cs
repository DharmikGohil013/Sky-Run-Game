using System.Collections.Generic;
using UnityEngine;

public class RoadSpawner : MonoBehaviour
{
    [Header("Road Settings")]
    [SerializeField] private GameObject roadPrefab;

    [SerializeField] private int roadCount = 5;

    [SerializeField] private float roadLength = 50f;
    [SerializeField] private float safeZone = 35f; // Safe distance behind player before deleting road

    [SerializeField] private Transform player;

    private float spawnZ = 0f;

    private List<GameObject> activeRoads =
        new List<GameObject>();

    private void Start()
    {
        // Spawn Starting Roads
        for (int i = 0; i < roadCount; i++)
        {
            // Do not spawn obstacles on the first 2 roads
            if (i < 2)
            {
                SpawnRoad(false);
            }
            else
            {
                SpawnRoad(true);
            }
        }
    }

    private void Update()
    {
        // Spawn New Road
        if (player.position.z - safeZone > 
            spawnZ - (roadCount * roadLength))
        {
            SpawnRoad();
            DeleteRoad();
        }
    }

    // =========================================
    // SPAWN ROAD
    // =========================================

    private void SpawnRoad(bool spawnObstacles = true)
    {
        GameObject road =
            Instantiate(
                roadPrefab,
                Vector3.forward * spawnZ,
                Quaternion.identity);

        // Disable obstacle spawner if requested
        if (!spawnObstacles)
        {
            ObstacleSpawner spawner = road.GetComponent<ObstacleSpawner>();
            if (spawner != null)
            {
                spawner.enabled = false;
            }
        }

        activeRoads.Add(road);

        spawnZ += roadLength;
    }

    // =========================================
    // DELETE OLD ROAD
    // =========================================

    private void DeleteRoad()
    {
        Destroy(activeRoads[0]);

        activeRoads.RemoveAt(0);
    }
}