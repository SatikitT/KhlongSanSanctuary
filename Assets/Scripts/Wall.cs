using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

public class Wall : MonoBehaviour
{
    public Tilemap topTile;
    public GameObject wallPrefab;
    public bool isActive = false; // Enables/disables wall placement

    public SpriteAtlas wallAtlas; // New reference for the single sprite sheet

    public GameObject shopScrollView;

    private List<Sprite> wallSprites = new List<Sprite>();

    private Vector3Int startCell;
    private Vector3Int endCell;
    private bool isPlacing = false;
    private List<GameObject> previewWalls = new List<GameObject>();
    public Dictionary<Vector3Int, GameObject> wallMap = new Dictionary<Vector3Int, GameObject>();
    private Dictionary<Vector3Int, GameObject> previewWallMap = new Dictionary<Vector3Int, GameObject>();

    void Start()
    {
        topTile = GameObject.Find("Ground").GetComponent<Tilemap>();

        if (wallAtlas != null)
        {
            LoadSpritesFromAtlas(); // Load sprites if atlas is already assigned
        }
    }

    public void LoadSpritesFromAtlas()
    {
        if (wallAtlas == null)
        {
            Debug.LogError("Wall Atlas is not assigned!");
            return;
        }

        // Extract all sprites from the assigned atlas
        Sprite[] sprites = new Sprite[wallAtlas.spriteCount];
        wallAtlas.GetSprites(sprites);

        // Store sprites in a list
        wallSprites.Clear();
        wallSprites.AddRange(sprites);
        wallSprites = wallSprites.OrderBy(sprite => ExtractNumberFromName(sprite.name)).ToList();
    }

    public Sprite GetWallSprite(int index)
    {
        if (index >= 0 && index < wallSprites.Count)
        {
            return wallSprites[index];
        }
        Debug.LogWarning($"Wall sprite index {index} is out of range!");
        return null;
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

            previewWalls.Add(wall);
            previewWallMap[cell] = wall;
        }

        // **Update sprites for preview walls**
        foreach (Vector3Int cell in previewWallMap.Keys)
        {
            UpdateWallSprite(previewWallMap[cell], cell, previewWallMap);
        }
    }

    private void ConfirmWallPlacement()
    {
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
        if (CanPlaceWall(cell))
        {
            Vector3 position = topTile.GetCellCenterWorld(cell);
            GameObject wall = Instantiate(wallPrefab, position, Quaternion.identity);
            wallMap[cell] = wall;
            UpdateWallSprite(wall, cell, wallMap);

            // Mark tile as occupied
            TilemapOccupationManager.Instance.MarkTileOccupied(cell);

            // Update adjacent walls immediately
            UpdateAdjacentWalls(cell);
        }
        else
        {
            Debug.Log("Cannot place wall: Tile is occupied!");
        }
    }



    public void UpdateAdjacentWalls(Vector3Int cell)
    {
        Vector3Int[] directions = { Vector3Int.up, Vector3Int.down, Vector3Int.left, Vector3Int.right };

        foreach (Vector3Int dir in directions)
        {
            Vector3Int neighborCell = cell + dir;
            if (wallMap.ContainsKey(neighborCell))
            {
                UpdateWallSprite(wallMap[neighborCell], neighborCell, wallMap);
            }
        }
    }


    private bool CanPlaceWall(Vector3Int cell)
    {
        return topTile.GetTile(cell) != null &&
               !TilemapOccupationManager.Instance.IsTileOccupied(cell) &&
               !IsMouseOverScrollView();
    }


    public void UpdateWallSprite(GameObject wall, Vector3Int cell, Dictionary<Vector3Int, GameObject> wallDictionary)
    {
        if (!wall || wallSprites.Count == 0) return;

        int left = wallDictionary.ContainsKey(cell + Vector3Int.left) ? 1 : 0;
        int right = wallDictionary.ContainsKey(cell + Vector3Int.right) ? 1 : 0;
        int up = wallDictionary.ContainsKey(cell + Vector3Int.up) ? 1 : 0;
        int down = wallDictionary.ContainsKey(cell + Vector3Int.down) ? 1 : 0;

        SpriteRenderer sr = wall.GetComponent<SpriteRenderer>();

        int spriteIndex = 0; // Default to single wall

        if (left == 1 && right == 1) spriteIndex = 2; // Horizontal middle
        else if (right == 1) spriteIndex = 1; // Horizontal end (Right)
        else if (left == 1) spriteIndex = 3; // Horizontal end (Left)
        else if (up == 1 && down == 1) spriteIndex = 11; // Vertical middle
        else if (down == 1) spriteIndex = 10; // Vertical end (Bottom)
        else if (up == 1) spriteIndex = 21; // Vertical end (Top)

        // Handle Angled Walls
        if (up == 1 && right == 1 && left == 0 && down == 0) spriteIndex = 18; // Top angled right
        if (up == 1 && right == 0 && left == 1 && down == 0) spriteIndex = 20; // Top angled left
        if (up == 0 && right == 1 && left == 0 && down == 1) spriteIndex = 7; // Bottom angled right
        if (up == 0 && right == 0 && left == 1 && down == 1) spriteIndex = 9; // Bottom angled left

        // Handle T-Junctions
        if (up == 0 && right == 1 && left == 1 && down == 1) spriteIndex = 4; // Top T
        if (up == 1 && right == 1 && left == 1 && down == 0) spriteIndex = 5; // Bottom T
        if (up == 1 && right == 0 && left == 1 && down == 1) { spriteIndex = 6; sr.flipX = true;  }; // Left T
        if (up == 1 && right == 1 && left == 0 && down == 1) spriteIndex = 6; // Right T

        // Fully connected cross-shape
        if (left == 1 && right == 1 && up == 1 && down == 1)
        {
            spriteIndex = 14; // Fully connected
            wall.transform.rotation = Quaternion.identity;
        }

        // Apply the selected sprite if within range
        if (spriteIndex < wallSprites.Count)
        {
            sr.sprite = wallSprites[spriteIndex];
        }
        else
        {
            Debug.LogWarning($"Sprite index {spriteIndex} is out of range!");
        }
    }



    private void ClearPreviewWalls()
    {
        foreach (GameObject wall in previewWalls)
        {
            Destroy(wall);
        }
        previewWalls.Clear();
        previewWallMap.Clear();
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

    public void DestroyWall(Vector3Int cell)
    {
        if (wallMap.ContainsKey(cell))
        {
            // Free the tile
            TilemapOccupationManager.Instance.MarkTileUnoccupied(cell);

            // Destroy the wall object
            Destroy(wallMap[cell]);
            wallMap.Remove(cell);

            Debug.Log("Wall removed at: " + cell);

            // Update adjacent walls to refresh their sprites
            UpdateAdjacentWalls(cell);
        }
    }

    private int ExtractNumberFromName(string name)
    {
        Match match = Regex.Match(name, @"\d+"); // Find number in the name
        return match.Success ? int.Parse(match.Value) : int.MaxValue; // If no number, push to the end
    }

    bool IsMouseOverScrollView()
    {
        if (EventSystem.current.IsPointerOverGameObject()) // Mouse is over UI
        {
            // Get the UI element the mouse is hovering over
            PointerEventData eventData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, results);

            foreach (RaycastResult result in results)
            {
                if (result.gameObject == shopScrollView) // If it's the ScrollView, return true
                {
                    return true;
                }
            }
        }
        return false; // Mouse is not over the ScrollView
    }

}
