using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class PromotionUI : MonoBehaviour
{
    [SerializeField]
    public const string gameObjectName = "PromotionUI(Clone)";

    [SerializeField]
    public bool isWhite;

    [SerializeField]
    public Vector3Int promotionTileIndex;

    [SerializeField]
    private Sprite[] whitePromotionSprites, blackPromotionSprites;

    [SerializeField]
    private GameObject tilePrefab, cancelPrefab;

    [SerializeField]
    private float tileSize = 1.0f;

    private Vector3Int up = new Vector3Int(0, 1, 1);
    private Vector3Int right = new Vector3Int(1, 1, 0);
    private Vector3Int forward = new Vector3Int(-1, 0, 1);

    private Vector2 xVector = Vector2.right;
    private Vector2 yVector = new Vector2(Mathf.Cos(Mathf.PI / 3), Mathf.Sin(Mathf.PI / 3));
    private Vector2 zVector = new Vector2(Mathf.Cos(Mathf.PI * 2 / 3), Mathf.Sin(Mathf.PI * 2 / 3));

    private void Awake()
    {
        gameObject.name = gameObjectName;
    }

    void Start()
    {
        SetupTiles();
    }

    private void SetupTiles()
    {
        Vector3 position = transform.position;

        Sprite[] promotionSprites = isWhite ? whitePromotionSprites : blackPromotionSprites;

        int direction = isWhite ? 1 : -1;

        int i = 0;
        foreach (Vector3Int index in new Vector3Int[] { Vector3Int.zero, -up, -right, -forward })
        {
            Vector3 tilePosition = IndexToOffset(direction * index) * tileSize;

            GameObject tile = Instantiate(tilePrefab, transform.TransformPoint(tilePosition), Quaternion.identity, transform);
            tile.transform.localScale *= tileSize * 2;

            Promote promote = tile.GetComponent<Promote>();
            promote.index = promotionTileIndex;
            promote.pieceToPromoteTo = IndexToPiece(index);

            GameObject piece = new GameObject("PromotionSprite");
            piece.transform.parent = transform;
            piece.transform.localPosition = tilePosition;
            piece.transform.localScale *= 0.2f * tileSize * 2;

            SpriteRenderer pieceSpriteRenderer = piece.AddComponent<SpriteRenderer>();
            pieceSpriteRenderer.sprite = promotionSprites[i];
            pieceSpriteRenderer.sortingLayerID = SortingLayer.NameToID("Pieces");
            pieceSpriteRenderer.sortingOrder = 3;

            i++;
        }

        GameObject cancel = Instantiate(cancelPrefab, transform.TransformPoint(IndexToOffset(direction * up) * tileSize), Quaternion.identity, transform);

        CancelPromotion cancelPromotion = cancel.GetComponent<CancelPromotion>();
        cancelPromotion.promotionUI = this;
    }

    private Vector3 IndexToOffset(Vector3Int index)
    {
        return index.x * xVector + index.y * yVector + index.z * zVector;
    }

    private byte IndexToPiece(Vector3 index)
    {
        byte piece = GlinskiChess.whitePawn;

        if (index == Vector3Int.zero)
        {
            piece = GlinskiChess.whiteQueen;
        }
        else if (index == -up)
        {
            piece = GlinskiChess.whiteKnight;
        }
        else if (index == -right)
        {
            piece = GlinskiChess.whiteBishop;
        }
        else if (index == -forward)
        {
            piece = GlinskiChess.whiteRook;
        }
        else
        {
            Debug.LogError("Invalid index: " + index);
        }

        if (!isWhite)
        {
            return (byte)(piece | GlinskiChess.black);
        }

        return piece;
    }

    public void CancelPromotion()
    {
        GameObject board = GameObject.Find("Board");
        GlinskiChess glinskiChessScript = board.GetComponent<GlinskiChess>();
        glinskiChessScript.CancelPromotion();
        Destroy(gameObject);
    }
}
