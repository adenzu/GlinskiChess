using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Vector3Int index;
    private GlinskiChess glinskiChess;
    private GameObject piece;
    private bool canDragPiece = false;
    private bool isPieceFlipped = false;

    private void Awake()
    {
        glinskiChess = transform.parent.GetComponent<GlinskiChess>();
    }

    public void SetPieceSprite(Sprite sprite)
    {
        if (piece != null)
        {
            piece.GetComponent<SpriteRenderer>().sprite = sprite;
        }
    }

    public void SetCanDragPiece(bool canDragPiece)
    {
        this.canDragPiece = canDragPiece;
    }

    public void RemovePiece()
    {
        Destroy(piece);
        piece = null;
    }

    public void SetPiece(GameObject piece)
    {
        RemovePiece();
        this.piece = piece;
        ResetPiecePosition();
        piece.transform.parent = transform;
    }

    public void MovePiece(Tile tile)
    {
        tile.SetPiece(piece);
        this.piece = null;
    }

    public void ResetPiecePosition()
    {
        if (piece != null)
        {
            piece.transform.position = transform.position;
        }
    }

    public void ResetPieceRotation()
    {
        if (piece != null)
        {
            piece.transform.rotation = isPieceFlipped ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;
        }
    }

    public void FlipPiece()
    {
        isPieceFlipped = !isPieceFlipped;
        ResetPieceRotation();
    }

    private void OnMouseEnter()
    {
        glinskiChess.HoverEnterTile(index);
    }

    private void OnMouseExit()
    {
        if (glinskiChess.GetHoveringTileIndex() == index)
        {
            glinskiChess.HoverExitTile();
        }
    }

    private void OnMouseDown()
    {
        if (piece != null)
        {
            SpriteRenderer spriteRenderer = piece.GetComponent<SpriteRenderer>();
            spriteRenderer.sortingLayerID = SortingLayer.NameToID("MoveIndicators");
            spriteRenderer.sortingOrder += 1;
        }
        glinskiChess.SelectPiece(index);
    }

    private void OnMouseDrag()
    {
        if (piece != null && canDragPiece)
        {
            piece.transform.position = Util.GetMousePosition();
        }
    }

    private void OnMouseUp()
    {
        if (piece != null)
        {
            SpriteRenderer spriteRenderer = piece.GetComponent<SpriteRenderer>();
            spriteRenderer.sortingLayerID = SortingLayer.NameToID("Pieces");
            spriteRenderer.sortingOrder -= 1;
        }
        glinskiChess.ReleasePiece();
    }
}
