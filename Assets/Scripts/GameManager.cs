using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public GameObject botPrefab;
    public Tilemap pathTilemap;
    private Path pathScript;
    private Vector3Int startPoint; // Set a valid start point
    private Vector3Int endPoint;   // Set a valid end point

    void Start()
    {
        pathScript = FindObjectOfType<Path>();
        StartCoroutine(SpawnBots());
    }

    IEnumerator SpawnBots()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f);

            if (pathScript.pathMap.Count == 0)
            {
                Debug.LogWarning("No path available to follow!");
                continue;
            }

            // Choose any random start and end from the pathMap
            Vector3Int startPoint = pathScript.pathMap.Keys.First();

            List<Vector3Int> randomPath = pathScript.GetRandomPath(startPoint);
            if (randomPath.Count == 0) continue; // No path found

            Vector3 spawnPosition = pathTilemap.GetCellCenterWorld(randomPath[0]);
            BotPathFollower bot = Instantiate(botPrefab, spawnPosition, Quaternion.identity).GetComponent<BotPathFollower>();
            bot.SetPath(randomPath, pathTilemap);
        }
    }

}
