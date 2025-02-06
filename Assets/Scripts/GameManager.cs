using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine.Tilemaps;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject botPrefab;
    public Tilemap pathTilemap;
    public Vector3Int entranceCell = new Vector3Int(0, -21, 0); // Fixed entrance point
    private Path pathScript;

    // List of walking animation clips for bots
    public List<AnimationClip> walkAnimations;

    void Start()
    {
        pathScript = FindObjectOfType<Path>();
        for (int i = 0; i < 12; i++)
        {
            pathScript.pathMap.Add(new Vector3Int(0, -21 + i, 0), null);
        }

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

            // Spawn a bot at the entrance
            Vector3 spawnPosition = pathTilemap.GetCellCenterWorld(entranceCell);
            GameObject botObject = Instantiate(botPrefab, spawnPosition, Quaternion.identity);

            // Assign a random walking animation
            AssignRandomWalkAnimation(botObject);

            // Initialize the bot
            BotPathFollower bot = botObject.GetComponent<BotPathFollower>();
            bot.Initialize(entranceCell, pathTilemap, this);
        }
    }

    void AssignRandomWalkAnimation(GameObject botObject)
    {
        Animator animator = botObject.GetComponent<Animator>();
        if (animator != null && walkAnimations.Count > 0)
        {
            // Create a new AnimatorOverrideController from the existing one
            AnimatorOverrideController overrideController = new AnimatorOverrideController(animator.runtimeAnimatorController);

            // Get the walking animation's original clip name (ensure this matches your Animator state name)
            string walkAnimationStateName = "Npc1";

            // Override the walking animation with a random one
            AnimationClip randomClip = walkAnimations[Random.Range(0, walkAnimations.Count)];
            overrideController[walkAnimationStateName] = randomClip;

            // Apply the override controller to the Animator
            animator.runtimeAnimatorController = overrideController;

            Debug.Log($"Assigned animation {randomClip.name} to bot {botObject.name}");
        }
        else
        {
            Debug.LogWarning("Animator or walkAnimations list is not set correctly!");
        }
    }

    public List<Vector3Int> GetPathBetween(Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> path = pathScript.GetPath(start, end);
        Debug.Log($"Path from {start} to {end}: {string.Join(" -> ", path)}");
        return path;
    }

    public List<Vector3Int> GetAvailableEndpoints(Vector3Int excludePoint, bool excludeSpawnPoint)
    {
        // Filter endpoints as cells with only one adjacent path
        return pathScript.pathMap.Keys
            .Where(cell => cell != excludePoint && (!excludeSpawnPoint || cell != entranceCell) && IsEndpoint(cell))
            .ToList();
    }

    private bool IsEndpoint(Vector3Int cell)
    {
        // Count the number of adjacent cells in the pathMap
        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        int adjacentCount = directions
            .Count(dir => pathScript.pathMap.ContainsKey(cell + dir));

        // Return true if there is only one adjacent path
        return adjacentCount == 1;
    }
}
