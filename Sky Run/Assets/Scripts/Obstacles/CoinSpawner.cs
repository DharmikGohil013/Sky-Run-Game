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

    private void Start() => SpawnCoins();

    private void SpawnCoins()
    {
        Vector3 parentScale = transform.lossyScale;
        Vector3 prefabScale = coinPrefab.transform.localScale;
        Vector3 correctedScale = new Vector3(
            prefabScale.x / parentScale.x,
            prefabScale.y / parentScale.y,
            prefabScale.z / parentScale.z
        );

        foreach (Transform point in spawnPoints)
        {
            if (Random.Range(0, 100) > spawnChance) continue;

            GameObject coin = Instantiate(coinPrefab, point.position, Quaternion.identity, transform);
            coin.transform.localScale = correctedScale;
        }
    }
}