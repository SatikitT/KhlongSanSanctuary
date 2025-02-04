using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public Dictionary<Vector3Int, GameObject> pathMap = new Dictionary<Vector3Int, GameObject>();
    private Dictionary<Vector3Int, GameObject> previewPathMap = new Dictionary<Vector3Int, GameObject>();

    void Start()
    {
        topTile = GameObject.Find("Ground").GetComponent<Tilemap>();
        pathMap.Add(new Vector3Int(0, -10, 0), null);
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

    private bool CanPlacePath(Vector3Int cell)
    {
        return topTile.GetTile(cell) != null && !TilemapOccupationManager.Instance.IsTileOccupied(cell);
    }

    private void PlacePath(Vector3Int cell)
    {
        if (CanPlacePath(cell))
        {
            Vector3 position = topTile.GetCellCenterWorld(cell);
            GameObject pathObj = Instantiate(pathPrefab, position, Quaternion.identity);
            pathMap[cell] = pathObj;
            UpdatePathSprite(pathObj, cell, pathMap);

            // Mark the tile as occupied
            TilemapOccupationManager.Instance.MarkTileOccupied(cell);

            // Update adjacent paths immediately
            UpdateAdjacentPaths(cell);
        }
        else
        {
            Debug.Log("Cannot place path here: Tile is occupied!");
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


    public void UpdatePathSprite(GameObject path, Vector3Int cell, Dictionary<Vector3Int, GameObject> pathDictionary)
    {
        if (!path) return;

        int left = pathDictionary.ContainsKey(cell + Vector3Int.left) ? 1 : 0;
        int right = pathDictionary.ContainsKey(cell + Vector3Int.right) ? 1 : 0;
        int up = pathDictionary.ContainsKey(cell + Vector3Int.up) ? 1 : 0;
        int down = pathDictionary.ContainsKey(cell + Vector3Int.down) ? 1 : 0;

        SpriteRenderer sr = path.GetComponent<SpriteRenderer>();

        int connectionCode = (up << 3) | (right << 2) | (left << 1) | down;
        path.transform.rotation = Quaternion.Euler(0, 0, 0);

        switch (connectionCode)
        {
            case 0b0000: // No connections
                sr.sprite = pathSprites[0]; // Single path
                break;
            case 0b0010: // Connected only to the right
                sr.sprite = pathSprites[1];
                path.transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
            case 0b0100: // Connected only to the left
                sr.sprite = pathSprites[1];
                path.transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case 0b1000: // Connected only up
                sr.sprite = pathSprites[4];
                break;
            case 0b0001: // Connected only down
                sr.sprite = pathSprites[3];
                break;
            case 0b0110: // Connected left and right
                sr.sprite = pathSprites[2];
                break;
            case 0b1001: // Connected up and down
                sr.sprite = pathSprites[5];
                break;
            //-------------------------------
            case 0b0111: // Top T-shape
                sr.sprite = pathSprites[6];
                path.transform.rotation = Quaternion.Euler(0, 0, -90);
                break;
            case 0b1110: // Bottom T-shape
                sr.sprite = pathSprites[7];
                path.transform.rotation = Quaternion.Euler(0, 0, 90);
                break;
            case 0b1101: // Left T-shape
                sr.sprite = pathSprites[8];
                break;
            case 0b1011: // Right T-shape
                sr.sprite = pathSprites[8];
                sr.flipX = true;
                break;
            //-------------------------------
            case 0b1100: // angle up right
                sr.sprite = pathSprites[10];
                break;
            case 0b0101: // Right right down
                sr.sprite = pathSprites[12];
                break;
            case 0b1010: // angle up left
                sr.sprite = pathSprites[11];
                break;
            case 0b0011: // angle down left
                sr.sprite = pathSprites[13];
                break;
            case 0b1111: // Fully connected (Cross)
                sr.sprite = pathSprites[9];
                path.transform.rotation = Quaternion.identity;
                break;
            default:
                sr.sprite = pathSprites[0];
                break;
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

    public void DestroyPath(Vector3Int cell)
    {
        if (pathMap.ContainsKey(cell))
        {
            // Free the tile from TilemapOccupationManager
            TilemapOccupationManager.Instance.MarkTileUnoccupied(cell);

            // Remove path object
            Destroy(pathMap[cell]);
            pathMap.Remove(cell);

            Debug.Log("Path removed at: " + cell);

            // Update adjacent paths
            UpdateAdjacentPaths(cell);
        }
    }

    public List<Vector3Int> GetPath(Vector3Int start, Vector3Int end)
    {
        if (!pathMap.ContainsKey(start) || !pathMap.ContainsKey(end))
            return new List<Vector3Int>(); // No path exists

        HashSet<Vector3Int> closedSet = new HashSet<Vector3Int>();
        HashSet<Vector3Int> openSet = new HashSet<Vector3Int>() { start };
        Dictionary<Vector3Int, Vector3Int> cameFrom = new Dictionary<Vector3Int, Vector3Int>();
        Dictionary<Vector3Int, float> gScore = pathMap.Keys.ToDictionary(k => k, k => float.MaxValue);
        Dictionary<Vector3Int, float> fScore = pathMap.Keys.ToDictionary(k => k, k => float.MaxValue);

        gScore[start] = 0;
        fScore[start] = Vector3Int.Distance(start, end);

        while (openSet.Count > 0)
        {
            Vector3Int current = openSet.OrderBy(n => fScore[n]).First();

            if (current == end)
                return ReconstructPath(cameFrom, current);

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (Vector3Int neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor)) continue;

                float tentativeGScore = gScore[current] + Vector3Int.Distance(current, neighbor);

                if (!openSet.Contains(neighbor))
                    openSet.Add(neighbor);

                if (tentativeGScore >= gScore[neighbor]) continue;

                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + Vector3Int.Distance(neighbor, end);
            }
        }

        return new List<Vector3Int>(); // No path found
    }

    // **?? Helper to Get Path**
    private List<Vector3Int> ReconstructPath(Dictionary<Vector3Int, Vector3Int> cameFrom, Vector3Int current)
    {
        List<Vector3Int> path = new List<Vector3Int> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();
        return path;
    }

    // **?? Get All Neighboring Tiles**
    private List<Vector3Int> GetNeighbors(Vector3Int cell)
    {
        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };
        return directions.Select(d => cell + d).Where(n => pathMap.ContainsKey(n)).ToList();
    }

    public List<Vector3Int> GetRandomPath(Vector3Int start)
    {
        if (!pathMap.ContainsKey(start))
        {
            Debug.LogError($"Start cell {start} is not in the pathMap!");
            return new List<Vector3Int>();
        }

        List<Vector3Int> path = new List<Vector3Int>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        Vector3Int current = start;
        path.Add(current);
        visited.Add(current);

        while (true)
        {
            List<Vector3Int> neighbors = GetNeighbors(current);

            // Remove already visited nodes to avoid loops
            neighbors.RemoveAll(n => visited.Contains(n));

            if (neighbors.Count == 0)
                break; // No more valid paths

            // Pick a random direction
            current = neighbors[Random.Range(0, neighbors.Count)];
            path.Add(current);
            visited.Add(current);
        }

        return path;
    }

}
