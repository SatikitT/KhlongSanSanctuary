using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
    public GameObject botPrefab;
    public Tilemap pathTilemap;
    public Path pathScript;

    void Start()
    {
        pathScript = FindObjectOfType<Path>();
        StartCoroutine(SpawnBots());
    }

    IEnumerator SpawnBots()
    {
        while (true)
        {
            yield return new WaitForSeconds(3f); // Spawns a bot every 3 seconds

            List<Vector3Int> userPathCells = pathScript.GetOrderedPath();
            if (userPathCells.Count == 0) continue; // No path exists, don't spawn

            Vector3 spawnPosition = pathTilemap.GetCellCenterWorld(userPathCells[0]); // Start at the first path cell
            BotPathFollower bot = Instantiate(botPrefab, spawnPosition, Quaternion.identity).GetComponent<BotPathFollower>();
            bot.SetPath(userPathCells, pathTilemap);
        }
    }
}
