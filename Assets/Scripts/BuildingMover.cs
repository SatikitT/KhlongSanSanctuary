using UnityEngine;

public class BuildingMover : MonoBehaviour
{
    public static BuildingMover Instance { get; private set; } // Singleton for global access
    private Building currentBuilding = null; // The building currently being dragged

    public bool isActive = false; // Whether the BuildingMover is active or not

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool IsDraggingBuilding()
    {
        return currentBuilding != null;
    }

    public void SetCurrentBuilding(Building building)
    {
        if (isActive && currentBuilding == null) // Allow setting current building only if active
        {
            currentBuilding = building;
        }
    }

    public void ClearCurrentBuilding()
    {
        currentBuilding = null;
    }

    private void Update()
    {
        if (!isActive)
        {
            return; // If deactivated, don't process dragging logic
        }

        if (currentBuilding != null && Input.GetMouseButtonUp(0))
        {
            currentBuilding = null; // Clear the reference when the mouse is released
        }
    }

    public void Activate()
    {
        isActive = true;
        Debug.Log("BuildingMover activated.");
    }

    public void Deactivate()
    {
        isActive = false;
        Debug.Log("BuildingMover deactivated.");
        ClearCurrentBuilding(); // Ensure no building remains selected when deactivated
    }
}
