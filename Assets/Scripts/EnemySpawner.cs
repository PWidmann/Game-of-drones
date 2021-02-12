using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private float spawnHeight = 35f;
    [SerializeField] private float spawnTimer = 1f;
    [SerializeField] Vector3 size;
    [SerializeField] int nodeCount;
    [SerializeField] private GameObject nodeObject;
    [SerializeField] private GameObject enemyObject;

    public List<GameObject> PatrolNodes;
    private GameObject patrolNode;
    private GameObject tempNode;
    private GameObject tempEnemy;
    private float timer;

    private void Awake()
    {
        CreateNodes();
    }

    void Start()
    {
        timer = spawnTimer;
        Time.timeScale = 1;
    }

    
    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            SpawnEnemy();
            timer = spawnTimer;
        }
    }

    void SpawnEnemy()
    {
        int rndX = (int)Random.Range(transform.position.x - (size.x / 2), transform.position.x + (size.x / 2));
        int rndZ = (int)Random.Range(transform.position.z - (size.z / 2), transform.position.z + (size.z / 2));

        tempEnemy = Instantiate(enemyObject, gameObject.transform);
        tempEnemy.transform.position = new Vector3(rndX, spawnHeight, rndZ);

        PlayerInterface.Instance.enemyCounter++;
    }

    void CreateNodes()
    {
        // Create node parent object
        patrolNode = Instantiate(new GameObject());
        patrolNode.name = "PatrolNodes";

        for (int i = 0; i < nodeCount; i++)
        {
            // Create node at random position inside spawn location
            int rndX = (int)Random.Range(transform.position.x - (size.x / 2), transform.position.x + (size.x / 2));
            int rndZ = (int)Random.Range(transform.position.z - (size.z / 2), transform.position.z + (size.z / 2));

            tempNode = Instantiate(nodeObject, patrolNode.transform);
            tempNode.transform.position = new Vector3(rndX, spawnHeight, rndZ);
            PatrolNodes.Add(tempNode);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, size);
    }
}
