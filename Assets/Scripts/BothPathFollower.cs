using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BotPathFollower : MonoBehaviour
{
    public float speed = 2f;
    private List<Vector3> pathPositions = new List<Vector3>();
    private int currentTargetIndex = 0;
    private bool moving = false;

    public void SetPath(List<Vector3Int> pathCells, Tilemap pathTilemap)
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
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Check if reached the target
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            currentTargetIndex++;

            if (currentTargetIndex >= pathPositions.Count)
            {
                moving = false;
            }
        }
    }
}
