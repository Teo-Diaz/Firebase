using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public GameObject obstaclePrefab;
    public int poolSize = 8;
    public float spawnRate = 2f;
    public float minY = -2f, maxY = 2f;

    private List<GameObject> obstaclePool;
    private float timeSinceLastSpawn;

    void Start()
    {
        obstaclePool = new List<GameObject>();

        for (int i = 0; i < poolSize; i++)
        {
            GameObject obstacle = Instantiate(obstaclePrefab, transform);
            obstacle.SetActive(false);
            obstaclePool.Add(obstacle);
        }
    }

    void Update()
    {
        timeSinceLastSpawn += Time.deltaTime;

        if (timeSinceLastSpawn >= spawnRate)
        {
            SpawnObstacle();
            timeSinceLastSpawn = 0f;
        }
    }

    void SpawnObstacle()
    {
        GameObject obstacle = GetPooledObstacle();

        if (obstacle != null)
        {
            float randomY = Random.Range(minY, maxY);
            obstacle.transform.position = new Vector2(10f, randomY);
            obstacle.SetActive(true);
        }
    }

    GameObject GetPooledObstacle()
    {
        foreach (GameObject obstacle in obstaclePool)
        {
            if (!obstacle.activeInHierarchy)
            {
                return obstacle;
            }
        }
        return null;
    }
}
