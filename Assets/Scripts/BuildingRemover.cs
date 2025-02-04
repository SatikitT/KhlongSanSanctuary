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
            RemoveBuilding();
        }
    }

    private void RemoveBuilding()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition);

        if (hitCollider != null)
        {
            Building building = hitCollider.GetComponentInParent<Building>();
            if (building != null)
            {
                building.DestroyBuilding();
                Debug.Log("Building Removed!");
                return;
            }
        }
    }
}
