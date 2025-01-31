using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Wall : MonoBehaviour
{
    public Tilemap topTile;
    public GameObject wallPrefab;
    public bool isActive = false; // Enables/disables wall placement

    public Sprite[] wallSprites; // 12 different sprites for walls

    private Vector3Int startCell;
    private Vector3Int endCell;
    private bool isPlacing = false;
    private List<GameObject> previewWalls = new List<GameObject>();
    private Dictionary<Vector3Int, GameObject> wallMap = new Dictionary<Vector3Int, GameObject>();

    void Start()
    {
        topTile = GameObject.Find("Ground").GetComponent<Tilemap>();
    }

    void Update()
    {
        if (!isActive) return;

        if (Input.GetMouseButtonDown(1)) // Right-click cancels preview
        {
            ClearPreviewWalls();
            isPlacing = false;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            StartPlacement();
        }
        else if (isPlacing && Input.GetMouseButton(0))
        {
            UpdateWallPreview();
        }
        else if (isPlacing && Input.GetMouseButtonUp(0)) // Instantly build on release
        {
            ConfirmWallPlacement();
        }
    }

    private void StartPlacement()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        startCell = topTile.WorldToCell(mouseWorldPos);

        if (topTile.GetTile(startCell) != null)
        {
            isPlacing = true;
            ShowWallPreview(startCell, startCell);
        }
    }

    private void UpdateWallPreview()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        endCell = topTile.WorldToCell(mouseWorldPos);

        ShowWallPreview(startCell, endCell);
    }

    private void ShowWallPreview(Vector3Int start, Vector3Int end)
    {
        ClearPreviewWalls();
        List<Vector3Int> path = GetCellsBetween(start, end);

        foreach (Vector3Int cell in path)
        {
            Vector3 position = topTile.GetCellCenterWorld(cell);
            GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity);
            SpriteRenderer sr = wall.GetComponent<SpriteRenderer>();
            sr.color = new Color(1f, 1f, 1f, 0.5f); // Transparent preview
            UpdateWallSprite(wall, cell);
            previewWalls.Add(wall);
        }
    }

    private void ConfirmWallPlacement()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        endCell = topTile.WorldToCell(mouseWorldPos);

        List<Vector3Int> path = GetCellsBetween(startCell, endCell);
        foreach (Vector3Int cell in path)
        {
            PlaceWall(cell);
        }

        ClearPreviewWalls();
        isPlacing = false;
    }

    private void PlaceWall(Vector3Int cell)
    {
        if (topTile.GetTile(cell) != null && !wallMap.ContainsKey(cell))
        {
            Vector3 position = topTile.GetCellCenterWorld(cell);
            GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity);
            wallMap[cell] = wall;
            UpdateWallSprite(wall, cell);

            // Update adjacent walls
            UpdateAdjacentWalls(cell);
        }
    }

    private void UpdateAdjacentWalls(Vector3Int cell)
    {
        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        foreach (Vector3Int dir in directions)
        {
            Vector3Int neighborCell = cell + dir;
            if (wallMap.ContainsKey(neighborCell))
            {
                UpdateWallSprite(wallMap[neighborCell], neighborCell);
            }
        }
    }

    private void UpdateWallSprite(GameObject wall, Vector3Int cell)
    {
        if (!wall) return;

        int left = wallMap.ContainsKey(cell + Vector3Int.left) ? 1 : 0;
        int right = wallMap.ContainsKey(cell + Vector3Int.right) ? 1 : 0;
        int up = wallMap.ContainsKey(cell + Vector3Int.up) ? 1 : 0;
        int down = wallMap.ContainsKey(cell + Vector3Int.down) ? 1 : 0;

        SpriteRenderer sr = wall.GetComponent<SpriteRenderer>();

        if (left == 1 && right == 1)
        {
            sr.sprite = wallSprites[2]; // Horizontal middle
        }
        else if (right == 1)
        {
            sr.sprite = wallSprites[1]; // Horizontal end (Right)
            sr.flipX = false;
        }
        else if (left == 1)
        {
            sr.sprite = wallSprites[1]; // Horizontal end (Left)
            sr.flipX = true;
        }
        else if (up == 1 && down == 1)
        {
            sr.sprite = wallSprites[5]; // Vertical middle
        }
        else if (down == 1)
        {
            sr.sprite = wallSprites[3]; // Vertical end (Bottom)
        }
        else if (up == 1)
        {
            sr.sprite = wallSprites[4]; // Vertical end (Top)
            return;
        }
        else
        {
            sr.sprite = wallSprites[0]; // Single wall
        }

        // Handle Angled Walls
        if (up == 1 && right == 1 && left == 0 && down == 0) sr.sprite = wallSprites[10]; // Top angled right
        if (up == 1 && right == 0 && left == 1 && down == 0) { sr.sprite = wallSprites[10]; sr.flipX = true; } // Top angled left
        if (up == 0 && right == 1 && left == 0 && down == 1) sr.sprite = wallSprites[11]; // Bottom angled right
        if (up == 0 && right == 0 && left == 1 && down == 1) { sr.sprite = wallSprites[11]; sr.flipX = true; } // Bottom angled left


        if (up == 0 && right == 1 && left == 1 && down == 1) sr.sprite = wallSprites[6]; // Top T
        if (up == 1 && right == 1 && left == 1 && down == 0) sr.sprite = wallSprites[7]; // Top T
        if (up == 1 && right == 0 && left == 1 && down == 1) { sr.sprite = wallSprites[8]; sr.flipX = true; } // Top T
        if (up == 1 && right == 1 && left == 0 && down == 1) { sr.sprite = wallSprites[8]; } // Top T


        // Fully connected cross-shape
        if (left == 1 && right == 1 && up == 1 && down == 1)
        {
            sr.sprite = wallSprites[9]; // Fully connected
            wall.transform.rotation = Quaternion.identity;
        }
    }


    private void ClearPreviewWalls()
    {
        foreach (GameObject wall in previewWalls)
        {
            Destroy(wall);
        }
        previewWalls.Clear();
    }

    private List<Vector3Int> GetCellsBetween(Vector3Int start, Vector3Int end)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        int x = start.x, y = start.y, dx = Mathf.Abs(end.x - x), dy = Mathf.Abs(end.y - y);
        int sx = x < end.x ? 1 : -1, sy = y < end.y ? 1 : -1, err = dx - dy;

        while (true)
        {
            path.Add(new Vector3Int(x, y, 0));
            if (x == end.x && y == end.y) break;
            int e2 = err * 2;
            if (e2 > -dy) { err -= dy; x += sx; }
            if (e2 < dx) { err += dx; y += sy; }
        }

        return path;
    }
}
