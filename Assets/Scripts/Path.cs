using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Path : MonoBehaviour
{
    public Tilemap topTile;
    public GameObject pathPrefab;
    public bool isActive = false; // Enables/disables path placement

    public Sprite[] pathSprites; // Different path sprites for various connections

    private Vector3Int startCell;
    private Vector3Int endCell;
    private bool isPlacing = false;
    private List<GameObject> previewPaths = new List<GameObject>();
    private Dictionary<Vector3Int, GameObject> pathMap = new Dictionary<Vector3Int, GameObject>();
    private Dictionary<Vector3Int, GameObject> previewPathMap = new Dictionary<Vector3Int, GameObject>();

    void Start()
    {
        topTile = GameObject.Find("Ground").GetComponent<Tilemap>();
    }

    void Update()
    {
        if (!isActive) return;

        if (Input.GetMouseButtonDown(1)) // Right-click cancels preview
        {
            ClearPreviewPaths();
            isPlacing = false;
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            StartPlacement();
        }
        else if (isPlacing && Input.GetMouseButton(0))
        {
            UpdatePathPreview();
        }
        else if (isPlacing && Input.GetMouseButtonUp(0)) // Instantly build on release
        {
            ConfirmPathPlacement();
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
            ShowPathPreview(startCell, startCell);
        }
    }

    private void UpdatePathPreview()
    {
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0f;
        endCell = topTile.WorldToCell(mouseWorldPos);

        ShowPathPreview(startCell, endCell);
    }

    private void ShowPathPreview(Vector3Int start, Vector3Int end)
    {
        ClearPreviewPaths();
        List<Vector3Int> path = GetCellsBetween(start, end);

        foreach (Vector3Int cell in path)
        {
            Vector3 position = topTile.GetCellCenterWorld(cell);
            GameObject pathObj = Instantiate(pathPrefab, position, Quaternion.identity);
            SpriteRenderer sr = pathObj.GetComponent<SpriteRenderer>();
            sr.color = new Color(1f, 1f, 1f, 0.5f); // Transparent preview

            previewPaths.Add(pathObj);
            previewPathMap[cell] = pathObj;
        }

        // **Update sprites for preview paths**
        foreach (Vector3Int cell in previewPathMap.Keys)
        {
            UpdatePathSprite(previewPathMap[cell], cell, previewPathMap);
        }
    }

    private void ConfirmPathPlacement()
    {
        List<Vector3Int> path = GetCellsBetween(startCell, endCell);
        foreach (Vector3Int cell in path)
        {
            PlacePath(cell);
        }

        ClearPreviewPaths();
        isPlacing = false;
    }

    private void PlacePath(Vector3Int cell)
    {
        if (topTile.GetTile(cell) != null && !pathMap.ContainsKey(cell))
        {
            Vector3 position = topTile.GetCellCenterWorld(cell);
            GameObject pathObj = Instantiate(pathPrefab, position, Quaternion.identity);
            pathMap[cell] = pathObj;
            UpdatePathSprite(pathObj, cell, pathMap);

            // Update adjacent paths
            UpdateAdjacentPaths(cell);
        }
    }

    private void UpdateAdjacentPaths(Vector3Int cell)
    {
        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        foreach (Vector3Int dir in directions)
        {
            Vector3Int neighborCell = cell + dir;
            if (pathMap.ContainsKey(neighborCell))
            {
                UpdatePathSprite(pathMap[neighborCell], neighborCell, pathMap);
            }
        }
    }

    private void UpdatePathSprite(GameObject path, Vector3Int cell, Dictionary<Vector3Int, GameObject> pathDictionary)
    {
        if (!path) return;

        int left = pathDictionary.ContainsKey(cell + Vector3Int.left) ? 1 : 0;
        int right = pathDictionary.ContainsKey(cell + Vector3Int.right) ? 1 : 0;
        int up = pathDictionary.ContainsKey(cell + Vector3Int.up) ? 1 : 0;
        int down = pathDictionary.ContainsKey(cell + Vector3Int.down) ? 1 : 0;

        SpriteRenderer sr = path.GetComponent<SpriteRenderer>();

        if (left == 1 && right == 1)
        {
            sr.sprite = pathSprites[2]; // Horizontal path
        }
        else if (up == 1 && down == 1)
        {
            sr.sprite = pathSprites[5]; // Vertical path
        }
        else if (right == 1)
        {
            sr.sprite = pathSprites[1]; // Path end (Right)
            sr.flipX = false;
        }
        else if (left == 1)
        {
            sr.sprite = pathSprites[1]; // Path end (Left)
            sr.flipX = true;
        }
        else if (down == 1)
        {
            sr.sprite = pathSprites[3]; // Path end (Bottom)
        }
        else if (up == 1)
        {
            sr.sprite = pathSprites[4]; // Path end (Top)
        }
        else
        {
            sr.sprite = pathSprites[0]; // Single path
        }

        // Handle T-Junctions
        if (up == 0 && right == 1 && left == 1 && down == 1) sr.sprite = pathSprites[6]; // Top T
        if (up == 1 && right == 1 && left == 1 && down == 0) sr.sprite = pathSprites[7]; // Bottom T
        if (up == 1 && right == 0 && left == 1 && down == 1) { sr.sprite = pathSprites[8]; sr.flipX = true; } // Left T
        if (up == 1 && right == 1 && left == 0 && down == 1) { sr.sprite = pathSprites[8]; } // Right T

        // Fully connected cross-shape
        if (left == 1 && right == 1 && up == 1 && down == 1)
        {
            sr.sprite = pathSprites[9]; // Fully connected
            path.transform.rotation = Quaternion.identity;
        }
    }

    private void ClearPreviewPaths()
    {
        foreach (GameObject path in previewPaths)
        {
            Destroy(path);
        }
        previewPaths.Clear();
        previewPathMap.Clear();
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
