using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingRemover : MonoBehaviour
{
    public bool isActive = false; // Enables/disables remover mode
    private Tilemap topTile;
    private Wall wallScript;
    private Path pathScript;

    void Start()
    {
        topTile = GameObject.Find("Ground").GetComponent<Tilemap>();

        // Get Wall and Path scripts from the same GameObject
        wallScript = GetComponent<Wall>();
        pathScript = GetComponent<Path>();

        if (wallScript == null || pathScript == null)
        {
            Debug.LogError("Wall or Path script is missing on BuildingRemover's GameObject!");
        }
    }

    void Update()
    {
        if (!isActive) return;

        if (Input.GetMouseButtonDown(0)) // Left-click to remove object
        {
            RemoveObjectAtMousePosition();
        }
    }

    private void RemoveObjectAtMousePosition()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        Vector3Int cell = topTile.WorldToCell(mouseWorldPos);

        Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorldPos);

        if (hitCollider != null)
        {
            GameObject objectToRemove = hitCollider.gameObject;

            Debug.Log($"Removing object at: {cell}");

            Building parentBuilding = objectToRemove.GetComponentInParent<Building>();
            if (parentBuilding != null)
            {
                parentBuilding.DestroyBuilding();
                return;
            }

            // Check if the clicked position contains a wall
            if (wallScript.wallMap.ContainsKey(cell))
            {
                wallScript.DestroyWall(cell); // Remove the wall
                UpdateAdjacentSprites(cell);  // Update neighbors
                return;
            }

            // Check if the clicked position contains a path
            if (pathScript.pathMap.ContainsKey(cell))
            {
                pathScript.DestroyPath(cell); // Remove the path
                UpdateAdjacentSprites(cell);  // Update neighbors
                return;
            }

            // If it's a different object, remove it normally
            TilemapOccupationManager.Instance.MarkTileUnoccupied(cell);
            Destroy(hitCollider.gameObject);
        }
    }

    private void UpdateAdjacentSprites(Vector3Int cell)
    {
        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        foreach (Vector3Int dir in directions)
        {
            Vector3Int neighborCell = cell + dir;

            if (wallScript.wallMap.ContainsKey(neighborCell))
            {
                wallScript.UpdateWallSprite(wallScript.wallMap[neighborCell], neighborCell, wallScript.wallMap);
            }

            if (pathScript.pathMap.ContainsKey(neighborCell))
            {
                pathScript.UpdatePathSprite(pathScript.pathMap[neighborCell], neighborCell, pathScript.pathMap);
            }
        }
    }
}
