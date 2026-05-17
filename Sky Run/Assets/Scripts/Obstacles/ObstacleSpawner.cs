using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle")]
    [SerializeField] private GameObject[] obstacles;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Settings")]
    [SerializeField] private int obstacleCount = 2;

    private void Start()
    {
        SpawnObstacles();
    }

    private void SpawnObstacles()
    {
        for (int i = 0; i < obstacleCount; i++)
        {
            // Random Spawn Point
            int spawnIndex =
                Random.Range(0, spawnPoints.Length);

            // Random Obstacle
            int obstacleIndex =
                Random.Range(0, obstacles.Length);

            // Spawn
            GameObject spawnedObstacle = Instantiate(
                obstacles[obstacleIndex],
                spawnPoints[spawnIndex].position,
                Quaternion.identity,
                transform);

            // Fix the scale so the obstacle doesn't get stretched by the RoadTile's size
            Vector3 parentScale = transform.lossyScale;
            Vector3 prefabScale = obstacles[obstacleIndex].transform.localScale;
            
            spawnedObstacle.transform.localScale = new Vector3(
                prefabScale.x / parentScale.x,
                prefabScale.y / parentScale.y,
                prefabScale.z / parentScale.z
            );
        }
    }
}
