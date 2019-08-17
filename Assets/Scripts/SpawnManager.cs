using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{

    public GameObject[] planes;
    public GameObject spawnPositionsParent;
    private List<Vector3> allPositions;
    private List<Vector3> usedPositions;

    public bool gameHasStarted = true;
    
    private float timeToReset = 0f;
    private int keyIndex = 0;
    private Dictionary<int, int> planesPerHalfMinute = new Dictionary<int, int>() // <index of half minute> <planes in that half minute>
    {
        {0, 4},
        {1, 6},
        {2, 8},
        {3, 7},
        {4, 6},
        {5, 10},
        {6, 6},
        {7, 8},
        {8, 9},
        {9, 6},
        {10, 8},
    };

    void Start()
    {
        if (!spawnPositionsParent)
        {
            Debug.LogWarning("Spawn Positions object is missing from the inspector");
        }
        else
        {
            GetAllPositions();
            usedPositions = new List<Vector3>();
        }
        if(planes == null)
        {
            Debug.LogWarning("No planes have been added to the planes array in the inspector");
        }
    }
    
    void Update()
    {
        if (gameHasStarted)
        {
            ManageTiming();
        }
    }   

    private void ManageTiming()
    {
        timeToReset -= Time.deltaTime;

        if(timeToReset < 0)
        {
            timeToReset = 30f;
            StartCoroutine(Spawn());
        }
    }

    IEnumerator Spawn()
    {
        float timeBetweenSpawn = 30f / planesPerHalfMinute[keyIndex];
        int planesToSpawn = planesPerHalfMinute[keyIndex];
        if (planesPerHalfMinute.ContainsKey(keyIndex + 1))
        {
            keyIndex++;
        }

        while (planesToSpawn > 0)
        {
            SpawnPlane();
            planesToSpawn--;
            yield return new WaitForSeconds(timeBetweenSpawn);
        }
    }

    private void GetAllPositions()
    {
        allPositions = new List<Vector3>();

        foreach(Transform child in spawnPositionsParent.transform)
        {
            Vector3 newPos = new Vector3(child.transform.position.x, child.transform.position.y, 0f);
            allPositions.Add(newPos);
        }
    }

    private void SpawnPlane()
    {
        GameObject plane = planes[Random.Range(0, planes.Length)];

        while (true)
        {
            Vector3 spawnPosition = allPositions[Random.Range(0, allPositions.Count)];

            if(!usedPositions.Contains(spawnPosition)){
                if(usedPositions.Count == 3)
                {
                    usedPositions.Remove(usedPositions.First());
                }
                usedPositions.Add(spawnPosition);

                Instantiate(plane, spawnPosition, Quaternion.identity);
                break;
            }
        }
        
    }

}
