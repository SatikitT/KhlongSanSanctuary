using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using TMPro;
using Unity.VisualScripting;
using System.Collections;

public enum ShopMode
{
    Building,
    Path,
    Wall,
    Environment,
    Mover,
    Destroyer
}


public class ShopManager : MonoBehaviour
{
    public List<GameObject> buildingPrefabs;
    public List<GameObject> pathPrefabs;
    public List<GameObject> wallPrefabs;
    public List<GameObject> environmentPrefabs;

    public Transform contentHolder; // Content area in the ScrollView
    public GameObject shopItemPrefab; // Prefab for shop item UI
    public Tilemap groundTilemap; // Reference to the ground tilemap
    public GameObject alignmentBlockPrefab; // Prefab for the placement block

    public Button buildingButton;
    public Button pathButton;
    public Button wallButton;
    public Button environmentButton;
    public Button removerButton;
    public Button destroyerButton;

    public BuildingRemover buildingRemover;
    public BuildingMover buildingMover;

    public TextMeshProUGUI shopInfoText; // Display text for remover/destroyer mode

    private GameObject selectedBuildingPrefab;
    private GameObject buildingInstance;
    private List<GameObject> placementBlocks = new List<GameObject>();
    private Camera mainCamera;
    private bool isDragging = false;

    public Image selectedButtonImage = null;
    public GameObject shopScrollView;
    public PlayerData playerData;

    private ShopMode currentMode = ShopMode.Building; // Default category

    void Start()
    {
        mainCamera = Camera.main;
        
        // Assign category buttons
        buildingButton.onClick.AddListener(() => ChangeCategory(ShopMode.Building));
        pathButton.onClick.AddListener(() => ChangeCategory(ShopMode.Path));
        wallButton.onClick.AddListener(() => ChangeCategory(ShopMode.Wall));
        environmentButton.onClick.AddListener(() => ChangeCategory(ShopMode.Environment));

        // Assign remover and destroyer mode buttons
        removerButton.onClick.AddListener(() => ActivateMode(ShopMode.Mover));
        destroyerButton.onClick.AddListener(() => ActivateMode(ShopMode.Destroyer));

        PopulateShop();
    }

    void ChangeCategory(ShopMode newCategory)
    {
        currentMode = newCategory;

        if (newCategory != ShopMode.Wall)
        {
            DeselectWall();
        }

        if (newCategory != ShopMode.Path)
        {
            DeselectPath();
        }

        PopulateShop();
    }


    void ActivateMode(ShopMode mode)
    {
        currentMode = mode;
        selectedBuildingPrefab = null;
        isDragging = false;

        if (mode != ShopMode.Wall)
        {
            DeselectWall();
        }

        if (mode != ShopMode.Path)
        {
            DeselectPath();
        }

        PopulateShop();
    }


    void PopulateShop()
    {
        foreach (Transform child in contentHolder)
        {
            Destroy(child.gameObject);
        }

        if (currentMode == ShopMode.Mover)
        {
            shopInfoText.text = "Mover Mode: Click and drag objects to move.";
            shopInfoText.gameObject.SetActive(true);
            buildingRemover.isActive = false;
            buildingMover.isActive = true;
            return;
        }
        else if (currentMode == ShopMode.Destroyer)
        {
            shopInfoText.text = "Destroyer Mode: Click objects to destroy.";
            shopInfoText.gameObject.SetActive(true);
            buildingMover.isActive = false;
            buildingRemover.isActive = true;
            return;
        }
        else
        {
            buildingMover.isActive = false;
            buildingRemover.isActive = false;
            shopInfoText.gameObject.SetActive(false);
        }

        if (currentMode == ShopMode.Wall)
        {
            foreach (GameObject wallPrefab in wallPrefabs)
            {
                WallObject wallObject = wallPrefab.GetComponent<WallObject>();
                if (wallObject == null) continue;

                GameObject shopItem = Instantiate(shopItemPrefab, contentHolder);
                ShopItemUI itemUI = shopItem.GetComponent<ShopItemUI>();

                if (itemUI != null)
                {
                    itemUI.SetupWall(wallPrefab);
                }

                Button button = shopItem.GetComponent<Button>();
                Image buttonImage = button.GetComponent<Image>();

                if (button != null)
                {
                    button.onClick.AddListener(() => StartPlacingWall(wallPrefab, buttonImage));
                }
            }
        }
        else if (currentMode == ShopMode.Path)
        {
            foreach (GameObject pathPrefab in pathPrefabs)
            {
                PathObject pathObject = pathPrefab.GetComponent<PathObject>();
                if (pathObject == null) continue;

                GameObject shopItem = Instantiate(shopItemPrefab, contentHolder);
                ShopItemUI itemUI = shopItem.GetComponent<ShopItemUI>();

                if (itemUI != null)
                {
                    itemUI.SetupPath(pathPrefab);
                }

                Button button = shopItem.GetComponent<Button>();
                Image buttonImage = button.GetComponent<Image>();

                if (button != null)
                {
                    button.onClick.AddListener(() => StartPlacingPath(pathPrefab, buttonImage));
                }
            }
        }
        else
        {
            // Default behavior for other categories (Buildings, Environment)
            List<GameObject> selectedCategory = GetSelectedCategory();

            foreach (GameObject prefab in selectedCategory)
            {
                GameObject shopItem = Instantiate(shopItemPrefab, contentHolder);
                ShopItemUI itemUI = shopItem.GetComponent<ShopItemUI>();

                if (itemUI != null)
                {
                    Building building = prefab.GetComponent<Building>();
                    itemUI.Setup(prefab, building.price, building.moneyPerPerson, building.faithPerPerson);
                }

                Button button = shopItem.GetComponent<Button>();
                Image buttonImage = button.GetComponent<Image>();

                if (button != null)
                {
                    button.onClick.AddListener(() => StartPlacingBuilding(prefab, buttonImage));
                }
            }
        }
    }



    List<GameObject> GetSelectedCategory()
    {
        switch (currentMode)
        {
            case ShopMode.Building:
                return buildingPrefabs;
            case ShopMode.Path:
                return pathPrefabs;
            case ShopMode.Wall:
                return wallPrefabs;
            case ShopMode.Environment:
                return environmentPrefabs;
            default:
                return new List<GameObject>(); // Empty list for invalid cases
        }
    }

    void Update()
    {
        HandleDraggingAndPlacement();

        if (Input.GetMouseButtonDown(0) && IsMouseOverScrollView())
        {
            if (currentMode == ShopMode.Wall)
            {
                DeselectWall();
            }
            else if (currentMode == ShopMode.Path)
            {
                DeselectPath();
            }
        }
    }


    void StartPlacingWall(GameObject wallPrefab, Image buttonImage)
    {
        // Reset previous selection visuals
        if (selectedButtonImage != null)
        {
            selectedButtonImage.color = Color.black;
        }

        buttonImage.color = Color.white;
        selectedButtonImage = buttonImage;

        WallObject wallObject = wallPrefab.GetComponent<WallObject>();
        if (wallObject == null)
        {
            Debug.LogError("Selected WallPrefab does not have a WallObject component!");
            return;
        }

        Debug.Log($"Selected Wall: {wallObject.wallName}");

        // Assign the selected WallObject to the Wall script
        Wall wallScript = FindObjectOfType<Wall>();
        if (wallScript != null)
        {
            wallScript.wallAtlas = wallObject.wallAtlas;
            wallScript.isActive = true; // Enable wall placement mode
            wallScript.LoadSpritesFromAtlas(); // Load sprites from the assigned atlas
        }
    }

    void StartPlacingPath(GameObject pathPrefab, Image buttonImage)
    {
        // Reset previous selection visuals
        if (selectedButtonImage != null)
        {
            selectedButtonImage.color = Color.black;
        }

        buttonImage.color = Color.white;
        selectedButtonImage = buttonImage;

        Debug.Log($"Selected Path: {pathPrefab.name}");

        Path pathScript = FindObjectOfType<Path>();
        if (pathScript != null)
        {
            pathScript.pathAtlas = pathPrefab.GetComponent<PathObject>().pathAtlas;
            pathScript.isActive = true;
            pathScript.LoadSpritesFromAtlas();
        }
    }


    void StartPlacingBuilding(GameObject buildingPrefab, Image buttonImage)
    {
        // Destroy previous building instance if it exists
        if (buildingInstance != null)
        {
            Destroy(buildingInstance);
        }

        // Reset previous button image to black
        if (selectedButtonImage != null)
        {
            selectedButtonImage.color = Color.black;
        }

        // Set new selected button image to white
        buttonImage.color = Color.white;
        selectedButtonImage = buttonImage;

        Debug.Log($"Selected Building: {buildingPrefab.name}");
        selectedBuildingPrefab = buildingPrefab;

        // Instantiate a new building instance
        buildingInstance = Instantiate(selectedBuildingPrefab);
        buildingInstance.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.5f); // Semi-transparent
        isDragging = true;
    }


    void HandleDraggingAndPlacement()
    {
        if (!isDragging || buildingInstance == null || groundTilemap == null)
            return;

        // Get mouse position
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        // Calculate the base cell position and snapped position
        Vector3Int baseCellPosition = groundTilemap.WorldToCell(mousePosition);
        Vector3 snappedPosition = CalculateSnappedPosition(baseCellPosition);

        // Update placement blocks
        ShowPlacementBlocks(snappedPosition);

        // Update building position
        buildingInstance.transform.position = mousePosition;

        if (Input.GetMouseButtonDown(0)) // Left-click to place
        {
            Building building = buildingInstance.GetComponent<Building>();
            if (building != null)
            {
                if (playerData.GetMoney() >= building.price)
                {
                    if (CanPlaceBuilding(snappedPosition) && !IsMouseOverScrollView())
                    {
                        PlaceBuilding(snappedPosition);
                    }
                    else
                    {
                        Debug.Log("Invalid placement position!");
                    }
                }
                else
                {
                    StartCoroutine(ShowInfoText("Not enough money!", Color.red));
                }
            }
        }
        else if (Input.GetMouseButtonDown(1)) // Right-click to cancel
        {
            CancelPlacement();
        }
    }

    IEnumerator ShowInfoText(string message, Color color)
    {
        shopInfoText.text = message;
        shopInfoText.color = color;
        shopInfoText.gameObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        shopInfoText.gameObject.SetActive(false);
        shopInfoText.color = Color.black; // Reset color to default
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


    // Calculate the snapped position for the building
    Vector3 CalculateSnappedPosition(Vector3Int baseCellPosition)
    {
        Building building = buildingInstance.GetComponent<Building>();
        if (building == null || building.shapeOffsets.Length == 0)
        {
            Debug.LogWarning("Building has no shape offsets defined.");
            return groundTilemap.GetCellCenterWorld(baseCellPosition);
        }

        // Use the first offset as the reference point
        Vector3 offsetReference = building.shapeOffsets[0];
        Vector3 baseCellWorldPosition = groundTilemap.GetCellCenterWorld(baseCellPosition);
        return baseCellWorldPosition - offsetReference;
    }

    bool CanPlaceBuilding(Vector3 snappedPosition)
    {
        Building building = buildingInstance.GetComponent<Building>();
        foreach (Vector3 offset in building.shapeOffsets)
        {
            Vector3Int offsetCell = groundTilemap.WorldToCell(snappedPosition + offset);

            // Check if the cell exists in the tilemap
            if (!groundTilemap.HasTile(offsetCell))
            {
                Debug.Log("Cannot place building outside the tilemap.");
                return false; // Prevent placement if out of bounds
            }

            // Check if the tile is occupied
            if (TilemapOccupationManager.Instance.IsTileOccupied(offsetCell))
            {
                return false; // If any tile is occupied, placement is invalid
            }
        }
        return true;
    }


    // Snap the building to placement blocks and finalize placement
    void PlaceBuilding(Vector3 snappedPosition)
    {
        Debug.Log($"Placed Building at {snappedPosition}");
        buildingInstance.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f); // Reset color
        playerData.AddMoney(-buildingInstance.GetComponent<Building>().price);
        Building building = buildingInstance.GetComponent<Building>();
        foreach (Vector3 offset in building.shapeOffsets)
        {
            Vector3Int cell = groundTilemap.WorldToCell(snappedPosition + offset);
            TilemapOccupationManager.Instance.MarkTileOccupied(cell); // Mark each tile as occupied
        }

        buildingInstance.transform.position = snappedPosition;
        buildingInstance = null;
        isDragging = false;
        
        HidePlacementBlocks();
    }

    // Cancel the placement
    public void CancelPlacement()
    {
        Debug.Log("Cancelled building placement.");
        Destroy(buildingInstance);
        buildingInstance = null;
        selectedButtonImage.color = Color.black;
        selectedButtonImage = null;
        isDragging = false;
        HidePlacementBlocks();
    }

    void ShowPlacementBlocks(Vector3 snappedPosition)
    {
        HidePlacementBlocks(); // Clear existing blocks

        Building building = buildingInstance.GetComponent<Building>();
        if (building == null) return;

        foreach (Vector3 offset in building.shapeOffsets)
        {
            Vector3Int offsetCell = groundTilemap.WorldToCell(snappedPosition + offset);
            Vector3 blockPosition = groundTilemap.GetCellCenterWorld(offsetCell);

            GameObject alignmentBlock = Instantiate(alignmentBlockPrefab, blockPosition, Quaternion.identity);
            SpriteRenderer renderer = alignmentBlock.GetComponent<SpriteRenderer>();

            bool isTileOutsideMap = groundTilemap.GetTile(offsetCell) == null; // Check if outside the map
            bool isTileOccupied = TilemapOccupationManager.Instance.IsTileOccupied(offsetCell); // Check if occupied

            if (renderer != null)
            {
                if (isTileOutsideMap || isTileOccupied)
                {
                    renderer.color = Color.red; // Outside map OR occupied = Red
                }
                else
                {
                    renderer.color = Color.green; // Otherwise = Green
                }
            }

            placementBlocks.Add(alignmentBlock);
        }
    }


    // Hide placement blocks
    void HidePlacementBlocks()
    {
        foreach (GameObject block in placementBlocks)
        {
            Destroy(block);
        }
        placementBlocks.Clear();
    }

    void DeselectWall()
    {
        Debug.Log("Wall deselected.");

        // Reset button UI
        if (selectedButtonImage != null)
        {
            selectedButtonImage.color = Color.black;
            selectedButtonImage = null;
        }

        // Disable wall placement
        Wall wallScript = FindObjectOfType<Wall>();
        if (wallScript != null)
        {
            wallScript.isActive = false;
        }
    }

    void DeselectPath()
    {
        Debug.Log("Path deselected.");

        // Reset button UI
        if (selectedButtonImage != null)
        {
            selectedButtonImage.color = Color.black;
            selectedButtonImage = null;
        }

        // Disable path placement
        Path pathScript = FindObjectOfType<Path>();
        if (pathScript != null)
        {
            pathScript.isActive = false;
        }
    }




}
