using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class SnapToGrid : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler           
{
    public int tileSize = 1;
    public Vector3 offset = Vector3.zero;

    public Tilemap topTile;

    SpriteRenderer spriteRenderer;
    Collider2D col;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();   
        col = GetComponent<Collider2D>();
        topTile = GameObject.Find("Tilemap").GetComponent<Tilemap>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        col.enabled = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 screenPoint = Input.mousePosition;
        screenPoint.z = 1f;
        transform.position = Camera.main.ScreenToWorldPoint(screenPoint);

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        col.enabled = true;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, Mathf.Infinity);

        Vector3Int cellPosition = topTile.LocalToCell(hit.point);
        transform.localPosition = topTile.GetCellCenterLocal(cellPosition);
    }

    void Update()
    {

    }
}
