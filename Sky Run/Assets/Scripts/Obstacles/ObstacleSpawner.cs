using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    [Header("Obstacle")]
    [SerializeField] private GameObject[] obstacles;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Settings")]
    [SerializeField] private int obstacleCount = 2;

    private void Start() => SpawnObstacles();

    private void SpawnObstacles()
    {
        for (int i = 0; i < obstacleCount; i++)
        {
            GameObject prefab = obstacles[Random.Range(0, obstacles.Length)];
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            GameObject spawned = Instantiate(prefab, spawnPoint.position, Quaternion.identity, transform);

            Vector3 parentScale = transform.lossyScale;
            Vector3 prefabScale = prefab.transform.localScale;

            spawned.transform.localScale = new Vector3(
                prefabScale.x / parentScale.x,
                prefabScale.y / parentScale.y,
                prefabScale.z / parentScale.z
            );
        }
    }
}