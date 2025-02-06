using System.Collections.Generic;
using System.Collections;
using UnityEngine.Tilemaps;
using UnityEngine;

public class BotPathFollower : MonoBehaviour
{
    public float speed = 2f;
    public int maxPathSwitches = 4; // Maximum number of path switches


    private List<Vector3> pathPositions = new List<Vector3>();
    private int currentTargetIndex = 0;
    private bool moving = false;
    private SpriteRenderer spriteRenderer;
    private int pathSwitchCount = 0; // Count of how many paths the bot has taken
    private Vector3Int currentCell; // Current cell position
    private Tilemap pathTilemap;
    private GameManager gameManager;

    public float detectionRadius = 1.0f;
    public LayerMask buildingLayer;
    private float interactionInterval = 5.0f;
    private PlayerData playerData;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerData = FindObjectOfType<PlayerData>();
        StartCoroutine(CheckForBuildings());
    }

    public void Initialize(Vector3Int entranceCell, Tilemap pathTilemap, GameManager gameManager)
    {
        this.currentCell = entranceCell; // Start at the entrance
        this.pathTilemap = pathTilemap;
        this.gameManager = gameManager;

        // Start the first path
        StartNewPath(true);
    }

    private void StartNewPath(bool excludeSpawnPoint)
    {
        Debug.Log($"Path Switch Count: {pathSwitchCount}");
        if (pathSwitchCount >= maxPathSwitches)
        {
            StartCoroutine(ReturnToEntranceAndDisappear());
            return;
        }

        // Get all available endpoints, excluding the spawn point if specified
        List<Vector3Int> availableEndpoints = gameManager.GetAvailableEndpoints(currentCell, excludeSpawnPoint);

        if (availableEndpoints.Count == 0)
        {
            Debug.LogWarning("No valid endpoints available!");
            StartCoroutine(ReturnToEntranceAndDisappear());
            return;
        }

        // Choose a random endpoint
        Vector3Int newEnd = availableEndpoints[Random.Range(0, availableEndpoints.Count)];
        List<Vector3Int> path = gameManager.GetPathBetween(currentCell, newEnd);

        if (path.Count > 0)
        {
            pathSwitchCount++;
            currentCell = newEnd; // Update the current cell to the endpoint
            SetPath(path);
        }
        else
        {
            Debug.LogWarning("No valid path found!");
            StartCoroutine(ReturnToEntranceAndDisappear());
        }
    }

    public void SetPath(List<Vector3Int> pathCells)
    {
        pathPositions.Clear();
        foreach (var cell in pathCells)
        {
            pathPositions.Add(pathTilemap.GetCellCenterWorld(cell));
        }

        if (pathPositions.Count > 0)
        {
            moving = true;
            currentTargetIndex = 0;
        }
    }

    void Update()
    {
        if (moving && pathPositions.Count > 0)
        {
            MoveAlongPath();
        }
    }

    private void MoveAlongPath()
    {
        Vector3 targetPosition = pathPositions[currentTargetIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // Rotate towards movement direction
        Vector3 direction = targetPosition - transform.position;
        if (direction.x > 0)
        {
            spriteRenderer.flipX = false; // Face right
        }
        else if (direction.x < 0)
        {
            spriteRenderer.flipX = true; // Face left
        }

        // Check if reached the target
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentTargetIndex++;

            if (currentTargetIndex >= pathPositions.Count)
            {
                moving = false;
                StartNewPath(false); // Allow spawn point as endpoint after the first path
            }
        }
    }

    private IEnumerator ReturnToEntranceAndDisappear()
    {
        moving = false;

        // Get the path back to the entrance
        List<Vector3Int> pathToEntrance = gameManager.GetPathBetween(currentCell, gameManager.entranceCell);

        if (pathToEntrance.Count > 0)
        {
            SetPath(pathToEntrance);

            while (true)
            {
                // Check if the bot's current tile position matches the entrance tile position
                Vector3Int currentTilePos = pathTilemap.WorldToCell(transform.position);
                if (currentTilePos == gameManager.entranceCell)
                {
                    break; // Exit the loop when the bot reaches the entrance
                }

                yield return null;
            }
        }

        Debug.Log("Bot has reached the entrance and will now disappear.");
        Destroy(gameObject); // Destroy the bot after it reaches the entrance
    }
    private IEnumerator CheckForBuildings()
    {
        while (true)
        {
            yield return new WaitForSeconds(interactionInterval);

            // Check for buildings within the detection radius
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectionRadius, buildingLayer);

            foreach (Collider2D hit in hits)
            {
                Building building = hit.GetComponent<Building>();
                if (building != null)
                {
                    // Interact with the building
                    playerData.AddMoney(building.moneyPerPerson);
                    playerData.AddFaith(building.faithPerPerson);
                    Debug.Log($"Interacted with {building.name}: Money +{building.moneyPerPerson}, Faith +{building.faithPerPerson}");
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        // Draw the detection radius in the editor
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
