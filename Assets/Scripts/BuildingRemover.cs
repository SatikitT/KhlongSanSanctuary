using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingRemover : MonoBehaviour
{
    public bool isActive = false;

    void Update()
    {
        if (!isActive) return;

        if (Input.GetMouseButtonDown(0))
        {
            RemoveBuildingOrObject();
        }
    }

    private void RemoveBuildingOrObject()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition);

        if (hitCollider != null)
        {
            // Check if it's a building
            Building building = hitCollider.GetComponentInParent<Building>();
            if (building != null)
            {
                building.DestroyBuilding();
                Debug.Log("Building Removed!");
                return;
            }

            // Check if it's a wall
            Wall wallManager = FindObjectOfType<Wall>(); // Assuming Wall is a singleton or unique object
            if (wallManager != null)
            {
                Vector3Int cell = wallManager.topTile.WorldToCell(mousePosition);
                if (wallManager.wallMap.ContainsKey(cell))
                {
                    wallManager.DestroyWall(cell);
                    Debug.Log($"Wall at {cell} Removed!");
                    return;
                }
            }

            // Check if it's a path
            Path pathManager = FindObjectOfType<Path>(); // Assuming Path is a singleton or unique object
            if (pathManager != null && !hitCollider.CompareTag("Untagged"))
            {
                Vector3Int cell = pathManager.topTile.WorldToCell(mousePosition);
                if (pathManager.pathMap.ContainsKey(cell))
                {
                    pathManager.DestroyPath(cell);
                    Debug.Log($"Path at {cell} Removed!");
                    return;
                }
            }

            Debug.Log("No removable object found.");
        }
    }
}
