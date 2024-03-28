using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Indicator : MonoBehaviour
{
    [SerializeField]
    private Sprite emptyTile, occupiedTile, checkTile;

    [SerializeField]
    private Color emptyTileColor, occupiedTileColor, checkTileColor;

    [SerializeField]
    private Vector3 emptyTileScale, emptyTileRotation, occupiedTileScale, occupiedTileRotation, checkTileScale, checkTileRotation;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void SetEmpty()
    {
        spriteRenderer.sprite = emptyTile;
        spriteRenderer.color = emptyTileColor;
        transform.localScale = emptyTileScale;
        transform.rotation = Quaternion.Euler(emptyTileRotation);
        spriteRenderer.sortingLayerID = SortingLayer.NameToID("MoveIndicators");
        spriteRenderer.sortingOrder -= 1;
    }

    public void SetOccupied()
    {
        spriteRenderer.sprite = occupiedTile;
        spriteRenderer.color = occupiedTileColor;
        transform.localScale = occupiedTileScale;
        transform.rotation = Quaternion.Euler(occupiedTileRotation);
        spriteRenderer.sortingLayerID = SortingLayer.NameToID("MoveIndicators");
        spriteRenderer.sortingOrder -= 1;
    }

    public void SetCheck()
    {
        spriteRenderer.sprite = checkTile;
        spriteRenderer.color = checkTileColor;
        transform.localScale = checkTileScale;
        transform.rotation = Quaternion.Euler(checkTileRotation);
        spriteRenderer.sortingLayerID = SortingLayer.NameToID("Tiles");
        spriteRenderer.sortingOrder += 1;
    }

    public bool IsCheck()
    {
        return spriteRenderer.sprite == checkTile;
    }
}
