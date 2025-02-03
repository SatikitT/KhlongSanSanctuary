using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TilemapOccupationManager : MonoBehaviour
{
    public static TilemapOccupationManager Instance { get; private set; }
    private HashSet<Vector3Int> occupiedTiles = new HashSet<Vector3Int>();

    void Awake()
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

    public bool IsTileOccupied(Vector3Int cell)
    {
        return occupiedTiles.Contains(cell);
    }

    public void MarkTileOccupied(Vector3Int cell)
    {
        occupiedTiles.Add(cell);
    }

    public void MarkTileUnoccupied(Vector3Int cell)
    {
        occupiedTiles.Remove(cell);
    }
}
