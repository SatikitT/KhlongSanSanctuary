using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Building : MonoBehaviour
{
    public Tilemap topTile;
    public Vector3[] shapeOffsets;
    public GameObject offsetBlock;
    public GameObject alignmentBlock;

    private List<GameObject> placementBlocks = new List<GameObject>(); // Store instantiated OffsetBlocks
    private bool dragging = false;
    private Vector3 dragOffset;
    private Vector3 previousPosition;
    private List<GameObject> blocks;
    private GameObject clickedChild;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        blocks = new List<GameObject>();
        clickedChild = null;
        topTile = GameObject.Find("Ground").GetComponent<Tilemap>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (shapeOffsets == null || shapeOffsets.Length == 0)
        {
            Debug.LogError("No shape offsets defined!");
        }

        // Instantiate blocks based on the offsets
        foreach (Vector3 offset in shapeOffsets)
        {
            var obj = Instantiate(offsetBlock, (Vector3)offset , Quaternion.identity);
            obj.transform.SetParent(transform, false);
            blocks.Add(obj);
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (!dragging)
            {
                DetectChildClick();
            }
            else
            {
                DragParent();
            }
        }
        if (dragging && Input.GetMouseButtonUp(0))
        {
            HandleMouseRelease();
        }

        if (dragging)
        {
            ShowPlacementBlocks();
        }
        else
        {
            HidePlacementBlocks();
        }
    }

    private void DetectChildClick()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition);

        if (hitCollider != null && hitCollider.transform.IsChildOf(transform))
        {
            Debug.Log($"Child {hitCollider.gameObject.name} was clicked!");

            previousPosition = transform.position;

            spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);

            clickedChild = hitCollider.gameObject;

            dragOffset = transform.position - mousePosition;

            Debug.Log($"Drag offset calculated: {dragOffset}");

            // Disable colliders for all child blocks
            foreach (GameObject obj in blocks)
            {
                obj.GetComponent<Collider2D>().enabled = false;
                Vector3Int childCellPos = topTile.LocalToCell(obj.transform.position);
                TilemapOccupationManager.Instance.MarkTileUnoccupied(childCellPos);
            }

            dragging = true;
        }
    }

    private void DragParent()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        // Move the parent while maintaining the drag offset
        transform.position = mousePosition + dragOffset;
    }

    private void HandleMouseRelease()
    {

        Transform clickedBox = clickedChild.transform;
        Debug.Log($"Child {clickedChild.name} was clicked!");

        Vector3 childWorldPosition = clickedBox.position;
        Vector3Int childCellPosition = topTile.LocalToCell(childWorldPosition);

        Vector3 childLocalOffset = clickedBox.localPosition;
        Vector3 parentTargetPosition = topTile.GetCellCenterLocal(childCellPosition) - childLocalOffset;

        if (CanPlaceBuilding(parentTargetPosition))
        {
            transform.position = parentTargetPosition;
            Debug.Log("Building Placed!");

            // Mark new tiles as occupied
            foreach (GameObject obj in blocks)
            {
                Vector3Int childCellPos = topTile.LocalToCell(obj.transform.position);
                TilemapOccupationManager.Instance.MarkTileOccupied(childCellPos);
            }
        }
        else
        {
            transform.position = previousPosition; // Reset to old position
            Debug.Log("Placement Failed: Tile occupied by another object.");

            // Re-mark old position as occupied since placement failed
            foreach (GameObject obj in blocks)
            {
                Vector3Int childCellPos = topTile.LocalToCell(obj.transform.position);
                TilemapOccupationManager.Instance.MarkTileOccupied(childCellPos);
            }
        }

        foreach (GameObject obj in blocks)
        {
            obj.GetComponent<Collider2D>().enabled = true;
        }

        dragging = false;
        spriteRenderer.color = new Color(1f, 1f, 1f, 1f);

        HidePlacementBlocks();
    }


    private bool CanPlaceBuilding(Vector3 parentTargetPosition)
    {
        foreach (GameObject obj in blocks)
        {
            Vector3 childTargetWorldPosition = parentTargetPosition + obj.transform.localPosition;
            Vector3Int childCellPosition = topTile.LocalToCell(childTargetWorldPosition);

            // Check if tile is occupied
            if (topTile.GetTile(childCellPosition) == null || TilemapOccupationManager.Instance.IsTileOccupied(childCellPosition))
            {
                return false; // Tile is either empty or occupied by a path
            }
        }
        return true;
    }


    private void ShowPlacementBlocks()
    {
        HidePlacementBlocks();

        foreach (Transform child in transform)
        {
            Vector3 childWorldPosition = child.position;
            Vector3Int childCellPosition = topTile.LocalToCell(childWorldPosition);
            Vector3 tileCenter = topTile.GetCellCenterLocal(childCellPosition);
            tileCenter = new Vector3(tileCenter.x, tileCenter.y, 1);

            // Instantiate OffsetBlock at the grid position
            GameObject block = Instantiate(alignmentBlock, tileCenter, Quaternion.identity);
            if (topTile.GetTile(childCellPosition) == null)
            {
                SpriteRenderer sp = block.GetComponent<SpriteRenderer>();
                sp.color = Color.red;
            }
            placementBlocks.Add(block);
        }
    }

    private void HidePlacementBlocks()
    {
        // Destroy all instantiated placement blocks
        foreach (GameObject block in placementBlocks)
        {
            Destroy(block);
        }
        placementBlocks.Clear();
    }

    public void DestroyBuilding()
    {
        // Free all occupied tiles
        foreach (GameObject obj in blocks)
        {
            Vector3Int childCellPos = topTile.LocalToCell(obj.transform.position);
            TilemapOccupationManager.Instance.MarkTileUnoccupied(childCellPos);
        }

        Destroy(gameObject);
    }

}
