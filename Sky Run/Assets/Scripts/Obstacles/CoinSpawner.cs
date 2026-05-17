using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    [Header("Coin")]
    [SerializeField] private GameObject coinPrefab;

    [Header("Spawn Points")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Spawn Chance")]
    [Range(0, 100)]
    [SerializeField] private int spawnChance = 70;

    private void Start()
    {
        SpawnCoins();
    }

    private void SpawnCoins()
    {
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            int randomChance = Random.Range(0, 100);

            if (randomChance <= spawnChance)
            {
                GameObject spawnedCoin = Instantiate(
                    coinPrefab,
                    spawnPoints[i].position,
                    Quaternion.identity,
                    transform);
                
                // Fix the scale so the coin doesn't get stretched by the RoadTile's size
                Vector3 parentScale = transform.lossyScale;
                Vector3 prefabScale = coinPrefab.transform.localScale;
                
                spawnedCoin.transform.localScale = new Vector3(
                    prefabScale.x / parentScale.x,
                    prefabScale.y / parentScale.y,
                    prefabScale.z / parentScale.z
                );
            }
        }
    }
}
