using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(AudioSource))]
public class GlinskiChess : MonoBehaviour
{
    public const byte whitePawn = 0b0000_0010;
    public const byte whiteKnight = 0b0000_0100;
    public const byte whiteBishop = 0b0000_1000;
    public const byte whiteRook = 0b0001_0000;
    public const byte whiteQueen = 0b0010_0000;
    public const byte whiteKing = 0b0100_0000;
    public const byte blackPawn = 0b0000_0011;
    public const byte blackKnight = 0b0000_0101;
    public const byte blackBishop = 0b0000_1001;
    public const byte blackRook = 0b0001_0001;
    public const byte blackQueen = 0b0010_0001;
    public const byte blackKing = 0b0100_0001;

    public const byte white = 0b0000_0000;
    public const byte black = 0b0000_0001;
    public const byte empty = 0b0000_0000;

    private HashSet<Vector3Int> enPassantableIndices = new HashSet<Vector3Int>();
    private List<HashSet<Vector3Int>> enPassantableIndicesHistory = new List<HashSet<Vector3Int>>();
    private HashSet<int> promotionHappened = new HashSet<int>();

    private Vector3Int[] everyValidTileIndex = { new Vector3Int(5, 0, 0), new Vector3Int(4, 0, 1), new Vector3Int(3, 0, 2), new Vector3Int(2, 0, 3), new Vector3Int(1, 0, 4), new Vector3Int(0, 0, 5), new Vector3Int(6, 1, 0), new Vector3Int(5, 1, 1), new Vector3Int(4, 1, 2), new Vector3Int(3, 1, 3), new Vector3Int(2, 1, 4), new Vector3Int(1, 1, 5), new Vector3Int(7, 2, 0), new Vector3Int(6, 2, 1), new Vector3Int(5, 2, 2), new Vector3Int(4, 2, 3), new Vector3Int(3, 2, 4), new Vector3Int(2, 2, 5), new Vector3Int(8, 3, 0), new Vector3Int(7, 3, 1), new Vector3Int(6, 3, 2), new Vector3Int(5, 3, 3), new Vector3Int(4, 3, 4), new Vector3Int(3, 3, 5), new Vector3Int(9, 4, 0), new Vector3Int(8, 4, 1), new Vector3Int(7, 4, 2), new Vector3Int(6, 4, 3), new Vector3Int(5, 4, 4), new Vector3Int(4, 4, 5), new Vector3Int(10, 5, 0), new Vector3Int(9, 5, 1), new Vector3Int(8, 5, 2), new Vector3Int(7, 5, 3), new Vector3Int(6, 5, 4), new Vector3Int(5, 5, 5), new Vector3Int(0, 1, 6), new Vector3Int(1, 2, 6), new Vector3Int(2, 3, 6), new Vector3Int(3, 4, 6), new Vector3Int(4, 5, 6), new Vector3Int(10, 6, 1), new Vector3Int(9, 6, 2), new Vector3Int(8, 6, 3), new Vector3Int(7, 6, 4), new Vector3Int(6, 6, 5), new Vector3Int(5, 6, 6), new Vector3Int(0, 2, 7), new Vector3Int(1, 3, 7), new Vector3Int(2, 4, 7), new Vector3Int(3, 5, 7), new Vector3Int(4, 6, 7), new Vector3Int(10, 7, 2), new Vector3Int(9, 7, 3), new Vector3Int(8, 7, 4), new Vector3Int(7, 7, 5), new Vector3Int(6, 7, 6), new Vector3Int(5, 7, 7), new Vector3Int(0, 3, 8), new Vector3Int(1, 4, 8), new Vector3Int(2, 5, 8), new Vector3Int(3, 6, 8), new Vector3Int(4, 7, 8), new Vector3Int(10, 8, 3), new Vector3Int(9, 8, 4), new Vector3Int(8, 8, 5), new Vector3Int(7, 8, 6), new Vector3Int(6, 8, 7), new Vector3Int(5, 8, 8), new Vector3Int(0, 4, 9), new Vector3Int(1, 5, 9), new Vector3Int(2, 6, 9), new Vector3Int(3, 7, 9), new Vector3Int(4, 8, 9), new Vector3Int(10, 9, 4), new Vector3Int(9, 9, 5), new Vector3Int(8, 9, 6), new Vector3Int(7, 9, 7), new Vector3Int(6, 9, 8), new Vector3Int(5, 9, 9), new Vector3Int(0, 5, 10), new Vector3Int(1, 6, 10), new Vector3Int(2, 7, 10), new Vector3Int(3, 8, 10), new Vector3Int(4, 9, 10), new Vector3Int(10, 10, 5), new Vector3Int(9, 10, 6), new Vector3Int(8, 10, 7), new Vector3Int(7, 10, 8), new Vector3Int(6, 10, 9), new Vector3Int(5, 10, 10) };

    private byte[,,] board = new byte[11, 11, 11];
    private int[,,] tileChildIndices = new int[11, 11, 11];
    private int[,,] moveIndicatorChildIndices = new int[11, 11, 11];
    private List<int> activeMoveIndicatorChildIndices = new List<int>();

    private Vector3Int up = new Vector3Int(0, 1, 1);
    private Vector3Int right = new Vector3Int(1, 1, 0);
    private Vector3Int forward = new Vector3Int(-1, 0, 1);

    private Vector2 xVector = Vector2.right;
    private Vector2 yVector = new Vector2(Mathf.Cos(Mathf.PI / 3), Mathf.Sin(Mathf.PI / 3));
    private Vector2 zVector = new Vector2(Mathf.Cos(Mathf.PI * 2 / 3), Mathf.Sin(Mathf.PI * 2 / 3));

    [SerializeField]
    private GameObject promotionUIPrefab;

    [SerializeField]
    private float tileSize = 1.0f;

    [SerializeField]
    private GameObject tilePrefab;

    [SerializeField]
    private GameObject moveIndicatorPrefab;

    [SerializeField]
    private GameObject piecePrefab;

    [SerializeField]
    private Sprite[] pieceSprites = new Sprite[12];

    [SerializeField]
    private Color[] tileColors = new Color[3] { new Color32(227, 218, 201, 255), Color.grey, new Color32(40, 40, 43, 255) };
    private int[,,] tileColorMap = new int[11, 11, 11];

    private AudioSource audioSource;

    [SerializeField]
    private AudioClip pieceMoveSFX, pieceCaptureSFX, checkSFX;

    private bool whiteTurn = true;
    private Vector3Int whiteKingPosition, blackKingPosition;

    private bool pieceSelected;
    private Vector3Int selectedPieceIndex;
    private Vector3Int hoveringTileIndex;
    private HashSet<Vector3Int> possibleMoves;

    private List<(Vector3Int, Vector3Int)> moveHistory = new List<(Vector3Int, Vector3Int)>();
    private List<(Vector3Int, byte)> captureHistory = new List<(Vector3Int, byte)>();
    private int pointInHistory = 0;

    [SerializeField]
    private bool localGame = false;

    [SerializeField]
    private GameObject localGameRestartButton;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        InitializeBoard();
        InstantiateTilesAndPieces();
        if (localGame)
        {
            UpdateLocalGameVisuals();
        }
        AlignCameraViewSizeWithBoard();
    }

    private void AlignCameraViewSizeWithBoard()
    {
        float boardWidth = tileSize * 2 * (10 * Mathf.Cos(Mathf.PI / 6));
        Camera.main.orthographicSize = boardWidth / (2 * Camera.main.aspect);
    }

    private Sprite GetPieceSprite(int piece)
    {
        switch (piece)
        {
            case whitePawn:
                return pieceSprites[0];
            case whiteKnight:
                return pieceSprites[1];
            case whiteBishop:
                return pieceSprites[2];
            case whiteRook:
                return pieceSprites[3];
            case whiteQueen:
                return pieceSprites[4];
            case whiteKing:
                return pieceSprites[5];
            case blackPawn:
                return pieceSprites[6];
            case blackKnight:
                return pieceSprites[7];
            case blackBishop:
                return pieceSprites[8];
            case blackRook:
                return pieceSprites[9];
            case blackQueen:
                return pieceSprites[10];
            case blackKing:
                return pieceSprites[11];
            default:
                return null;
        }
    }

    private void InitializeBoard()
    {
        Vector3Int boardCenter = new Vector3Int(5, 5, 5);

        // Initialize white pieces

        // Initialize white pawns
        board[5, 4, 4] = whitePawn;
        for (int i = 1; i < 5; i++)
        {
            board[5 - i, 4 - i, 4] = whitePawn;
            board[5 + i, 4, 4 - i] = whitePawn;
        }

        // Initialize white knights
        Vector3Int firstKnightPosition = boardCenter - 3 * up - 2 * right;
        Vector3Int secondKnightPosition = boardCenter - 3 * up - 2 * forward;
        board[firstKnightPosition.x, firstKnightPosition.y, firstKnightPosition.z] = whiteKnight;
        board[secondKnightPosition.x, secondKnightPosition.y, secondKnightPosition.z] = whiteKnight;

        // Initialize white bishops
        Vector3Int bishopPosition = boardCenter - 3 * up;
        board[bishopPosition.x, bishopPosition.y, bishopPosition.z] = whiteBishop; bishopPosition -= up;
        board[bishopPosition.x, bishopPosition.y, bishopPosition.z] = whiteBishop; bishopPosition -= up;
        board[bishopPosition.x, bishopPosition.y, bishopPosition.z] = whiteBishop;

        // Initialize white rooks
        Vector3Int firstRookPosition = boardCenter - 2 * up - 3 * right;
        Vector3Int secondRookPosition = boardCenter - 2 * up - 3 * forward;
        board[firstRookPosition.x, firstRookPosition.y, firstRookPosition.z] = whiteRook;
        board[secondRookPosition.x, secondRookPosition.y, secondRookPosition.z] = whiteRook;

        // Initialize white queen
        Vector3Int queenPosition = boardCenter - 4 * up - right;
        board[queenPosition.x, queenPosition.y, queenPosition.z] = whiteQueen;

        // Initialize white king
        Vector3Int kingPosition = boardCenter - 4 * up - forward;
        board[kingPosition.x, kingPosition.y, kingPosition.z] = whiteKing;
        whiteKingPosition = kingPosition;

        // Initialize black pieces
        // Initialize black pawns
        board[5, 6, 6] = blackPawn;
        for (int i = 1; i < 5; i++)
        {
            board[5 + i, 6 + i, 6] = blackPawn;
            board[5 - i, 6, 6 + i] = blackPawn;
        }

        // Initialize black knights
        firstKnightPosition = boardCenter + 3 * up + 2 * right;
        secondKnightPosition = boardCenter + 3 * up + 2 * forward;
        board[firstKnightPosition.x, firstKnightPosition.y, firstKnightPosition.z] = blackKnight;
        board[secondKnightPosition.x, secondKnightPosition.y, secondKnightPosition.z] = blackKnight;

        // Initialize black bishops
        bishopPosition = boardCenter + 3 * up;
        board[bishopPosition.x, bishopPosition.y, bishopPosition.z] = blackBishop; bishopPosition += up;
        board[bishopPosition.x, bishopPosition.y, bishopPosition.z] = blackBishop; bishopPosition += up;
        board[bishopPosition.x, bishopPosition.y, bishopPosition.z] = blackBishop;

        // Initialize black rooks
        firstRookPosition = boardCenter + 2 * up + 3 * right;
        secondRookPosition = boardCenter + 2 * up + 3 * forward;
        board[firstRookPosition.x, firstRookPosition.y, firstRookPosition.z] = blackRook;
        board[secondRookPosition.x, secondRookPosition.y, secondRookPosition.z] = blackRook;

        // Initialize black queen
        queenPosition = boardCenter + 4 * up + forward;
        board[queenPosition.x, queenPosition.y, queenPosition.z] = blackQueen;

        // Initialize black king
        kingPosition = boardCenter + 4 * up + right;
        board[kingPosition.x, kingPosition.y, kingPosition.z] = blackKing;
        blackKingPosition = kingPosition;
    }

    private void InstantiateTilesAndPieces()
    {
        foreach (Vector3Int index in everyValidTileIndex)
        {
            Vector3 offset = IndexToOffset(index);

            GameObject moveIndicator = Instantiate(moveIndicatorPrefab, transform.position + tileSize * offset, Quaternion.identity, transform);
            moveIndicator.transform.localScale *= tileSize * 2;
            moveIndicator.SetActive(false);
            moveIndicatorChildIndices[index.x, index.y, index.z] = transform.childCount - 1;

            GameObject tileObject = Instantiate(tilePrefab, transform.position + tileSize * offset, Quaternion.identity, transform);
            tileObject.GetComponent<SpriteRenderer>().color = GetTileColor(index);
            tileObject.transform.localScale = Vector3.one * tileSize * 2;
            Tile tile = tileObject.GetComponent<Tile>();
            tile.index = index;
            tileChildIndices[index.x, index.y, index.z] = transform.childCount - 1;

            Sprite pieceSprite = GetPieceSprite(GetPiece(index));

            if (pieceSprite != null)
            {
                GameObject pieceObject = Instantiate(piecePrefab, transform.position + tileSize * offset, Quaternion.identity, transform);
                SpriteRenderer pieceRenderer = pieceObject.GetComponent<SpriteRenderer>();
                pieceRenderer.sprite = pieceSprite;
                tile.SetPiece(pieceObject);
            }
        }

    }

    [ContextMenu("Flip")]
    private void Flip()
    {
        transform.RotateAround(transform.position, Vector3.forward, 180);
        foreach (Vector3Int index in everyValidTileIndex)
        {
            Tile tile = GetTile(index);
            tile.ResetPieceRotation();
        }
    }

    private Vector2 IndexToOffset(int x, int y, int z)
    {
        x -= 5; y -= 5; z -= 5;
        return x * xVector + y * yVector + z * zVector;
    }

    private Vector2 IndexToOffset(Vector3Int index)
    {
        return IndexToOffset(index.x, index.y, index.z);
    }

    public void HoverEnterTile(Vector3Int index)
    {
        hoveringTileIndex = index;
    }

    public Vector3Int GetHoveringTileIndex()
    {
        return hoveringTileIndex;
    }

    public void HoverExitTile()
    {
        hoveringTileIndex = -Vector3Int.one;
    }

    public void SelectPiece(Vector3Int index)
    {
        byte piece = GetPiece(index);
        byte pieceColor = GetPieceColor(piece);

        possibleMoves = GetPossibleMovesFiltered(index.x, index.y, index.z);

        if (pieceColor != (whiteTurn ? white : black) || possibleMoves.Count == 0)
        {
            GetTile(index).SetCanDragPiece(false);
            return;
        }

        GetTile(index).SetCanDragPiece(true);

        if (activeMoveIndicatorChildIndices.Count > 0)
        {
            HideMoves();
        }

        ShowMoves(possibleMoves);
        selectedPieceIndex = index;
        pieceSelected = true;
    }

    public void ReleasePiece()
    {
        if (!pieceSelected)
        {
            return;
        }
        if (activeMoveIndicatorChildIndices.Count > 0)
        {
            HideMoves();
        }
        if (possibleMoves.Contains(hoveringTileIndex))
        {
            MovePiece(selectedPieceIndex, hoveringTileIndex);
        }
        else
        {
            GetTile(selectedPieceIndex).ResetPiecePosition();
        }
        pieceSelected = false;
        selectedPieceIndex = -Vector3Int.one;
        possibleMoves.Clear();
    }

    private void ShowMoves(HashSet<Vector3Int> moves)
    {
        foreach (Vector3Int move in moves)
        {
            int moveIndicatorChildIndex = moveIndicatorChildIndices[move.x, move.y, move.z];
            Transform indicatorChild = transform.GetChild(moveIndicatorChildIndex);
            if (GetPiece(move) != empty)
            {
                indicatorChild.GetComponent<Indicator>().SetOccupied();
            }
            else
            {
                indicatorChild.GetComponent<Indicator>().SetEmpty();
            }
            indicatorChild.gameObject.SetActive(true);
            activeMoveIndicatorChildIndices.Add(moveIndicatorChildIndex);
        }
    }

    private void HideMoves()
    {
        foreach (int activeMoveIndicatorChildIndex in activeMoveIndicatorChildIndices)
        {
            transform.GetChild(activeMoveIndicatorChildIndex).gameObject.SetActive(false);
        }
        activeMoveIndicatorChildIndices.Clear();
    }

    private void MovePiece(Vector3Int from, Vector3Int to, bool writesToHistory = true)
    {
        byte movedPiece = GetPiece(from);
        byte capturedPiece = GetPiece(to);

        Vector3Int capturedPieceIndex = to;

        if (enPassantableIndices.Contains(to))
        {
            Vector3Int offset = GetPieceColor(movedPiece) == white ? -up : up;
            capturedPieceIndex += offset;
            capturedPiece = GetPiece(capturedPieceIndex);
            board[to.x, to.y, to.z] = movedPiece;
            board[capturedPieceIndex.x, capturedPieceIndex.y, capturedPieceIndex.z] = empty;
            board[from.x, from.y, from.z] = empty;
            GetTile(capturedPieceIndex).RemovePiece();
        }
        else
        {
            board[to.x, to.y, to.z] = movedPiece;
            board[from.x, from.y, from.z] = empty;
        }

        if (writesToHistory)
        {
            enPassantableIndicesHistory.Add(enPassantableIndices);
            enPassantableIndices = new HashSet<Vector3Int>();
        }

        bool promoteInProcess = false;

        if (movedPiece == whitePawn)
        {
            if (IsPromoteTileForWhite(to))
            {
                promoteInProcess = true;
            }
            else if ((to - from) == 2 * up)
            {
                enPassantableIndices.Add(to - up);
            }
        }
        else if (movedPiece == blackPawn)
        {
            if (IsPromoteTileForBlack(to))
            {
                promoteInProcess = true;
            }
            else if ((to - from) == 2 * -up)
            {
                enPassantableIndices.Add(to + up);
            }
        }

        GetTile(from).MovePiece(GetTile(to));

        if (capturedPiece != empty)
        {
            audioSource.PlayOneShot(pieceCaptureSFX);
        }
        else
        {
            audioSource.PlayOneShot(pieceMoveSFX);
        }

        if (movedPiece == whiteKing)
        {
            GetMoveIndicator(whiteKingPosition).gameObject.SetActive(false);
            whiteKingPosition = to;
        }
        else if (movedPiece == blackKing)
        {
            GetMoveIndicator(blackKingPosition).gameObject.SetActive(false);
            blackKingPosition = to;
        }
        else if (IsBlackKingInCheck())
        {
            audioSource.PlayOneShot(checkSFX);
            GetMoveIndicatorAfterActivating(blackKingPosition).SetCheck();
            if (HasWhiteWon())
            {
                localGameRestartButton.SetActive(true);
            }
        }
        else if (IsWhiteKingInCheck())
        {
            audioSource.PlayOneShot(checkSFX);
            GetMoveIndicatorAfterActivating(whiteKingPosition).SetCheck();
            if (HasBlackWon())
            {
                localGameRestartButton.SetActive(true);
            }
        }

        if (!promoteInProcess)
        {
            whiteTurn = !whiteTurn;
        }
        else
        {
            Vector3 offset = IndexToOffset(to);
            GameObject promotionUIInstance = Instantiate(promotionUIPrefab, transform.TransformPoint(offset * tileSize), transform.localRotation, transform);
            PromotionUI promotionUI = promotionUIInstance.GetComponent<PromotionUI>();
            promotionUI.isWhite = GetPieceColor(movedPiece) == white;
            promotionUI.promotionTileIndex = to;
        }

        if (writesToHistory)
        {
            moveHistory.RemoveRange(pointInHistory, moveHistory.Count - pointInHistory);
            captureHistory.RemoveRange(pointInHistory, captureHistory.Count - pointInHistory);
            promotionHappened.RemoveWhere((int index) => index > pointInHistory);

            moveHistory.Add((from, to));
            captureHistory.Add((capturedPieceIndex, capturedPiece));
        }

        pointInHistory++;
    }

    [ContextMenu("NextMove")]
    public void MoveForwardInHistory()
    {
        if (pointInHistory == moveHistory.Count)
        {
            return;
        }

        (Vector3Int, Vector3Int) move = moveHistory[pointInHistory];

        Vector3Int from = move.Item1;
        Vector3Int to = move.Item2;

        MovePiece(from, to, false);
    }

    [ContextMenu("PreviousMove")]
    public void MoveBackwardInHistory()
    {
        if (pointInHistory == 0)
        {
            return;
        }

        whiteTurn = !whiteTurn;
        pointInHistory--;

        (Vector3Int, Vector3Int) move = moveHistory[pointInHistory];
        (Vector3Int, byte) capture = captureHistory[pointInHistory];

        Vector3Int from = move.Item1;
        Vector3Int to = move.Item2;

        board[from.x, from.y, from.z] = GetPiece(to);
        GetTile(to).MovePiece(GetTile(from));

        Vector3Int capturedPieceIndex = capture.Item1;
        byte capturedPiece = capture.Item2;

        board[capturedPieceIndex.x, capturedPieceIndex.y, capturedPieceIndex.z] = capturedPiece;

        GameObject capturedPieceObject = Instantiate(piecePrefab);
        capturedPieceObject.GetComponent<SpriteRenderer>().sprite = GetPieceSprite(capturedPiece);
        GetTile(capturedPieceIndex).SetPiece(capturedPieceObject);

        enPassantableIndices = enPassantableIndicesHistory[pointInHistory];

        if (promotionHappened.Contains(pointInHistory + 1))
        {
            byte pawn = IsWhite(GetPiece(from)) ? whitePawn : blackPawn;
            board[from.x, from.y, from.z] = pawn;
            GetTile(from).SetPieceSprite(GetPieceSprite(pawn));
            promotionHappened.Remove(pointInHistory + 1);
        }
    }

    private void UndoLastMove()
    {
        if (moveHistory.Count == 0)
        {
            return;
        }

        MoveBackwardInHistory();

        moveHistory.RemoveAt(moveHistory.Count - 1);
        captureHistory.RemoveAt(captureHistory.Count - 1);
        enPassantableIndicesHistory.RemoveAt(enPassantableIndicesHistory.Count - 1);
    }

    private Tile GetTile(Vector3Int index)
    {
        return transform.GetChild(tileChildIndices[index.x, index.y, index.z]).GetComponent<Tile>();
    }

    private Indicator GetMoveIndicator(Vector3Int index)
    {
        return transform.GetChild(moveIndicatorChildIndices[index.x, index.y, index.z]).GetComponent<Indicator>();
    }

    private Indicator GetMoveIndicatorAfterActivating(Vector3Int index)
    {
        int moveIndicatorChildIndex = moveIndicatorChildIndices[index.x, index.y, index.z];
        Transform indicatorChild = transform.GetChild(moveIndicatorChildIndex);
        indicatorChild.gameObject.SetActive(true);
        return indicatorChild.GetComponent<Indicator>();
    }

    private void SetMoveIndicatorActivityFromPredicate(Vector3Int index, System.Func<Indicator, bool> Predicate)
    {
        Indicator indicator = GetMoveIndicator(index);
        indicator.gameObject.SetActive(Predicate(indicator));
    }

    private HashSet<Vector3Int> GetPossibleMovesFiltered(int x, int y, int z)
    {
        int piece = GetPiece(x, y, z);
        if (piece == empty)
        {
            return new HashSet<Vector3Int>();
        }
        else if (piece == whitePawn)
        {
            return GetPossibleMovesWhitePawnFiltered(x, y, z);
        }
        else if (piece == blackPawn)
        {
            return GetPossibleMovesBlackPawnFiltered(x, y, z);
        }
        else switch (piece | 0b0000_0001)
            {
                case blackKnight:
                    return GetPossibleMovesKnightFiltered(x, y, z);
                case blackBishop:
                    return GetPossibleMovesBishopFiltered(x, y, z);
                case blackRook:
                    return GetPossibleMovesRookFiltered(x, y, z);
                case blackQueen:
                    return GetPossibleMovesQueenFiltered(x, y, z);
                case blackKing:
                    return GetPossibleMovesKingFiltered(x, y, z);
                default:
                    return new HashSet<Vector3Int>();
            }
    }

    private HashSet<Vector3Int> GetPossibleMovesWhitePawnFiltered(int x, int y, int z, bool checkKingInCheck = true)
    {
        HashSet<Vector3Int> moves = new HashSet<Vector3Int>();
        Vector3Int pawnPosition = new Vector3Int(x, y, z);

        Vector3Int oneUp = pawnPosition + this.up;
        Vector3Int twoUp = pawnPosition + 2 * this.up;
        Vector3Int oneRight = pawnPosition + this.right;
        Vector3Int oneForward = pawnPosition + this.forward;

        if (IsOnBoard(oneUp) && GetPiece(oneUp) == empty)
        {
            if (IsOnBoard(twoUp) && GetPiece(twoUp) == empty && CanWhitePawnMoveTwoTiles(x, y, z))
            {
                moves.Add(twoUp);
            }
            moves.Add(oneUp);
        }

        System.Func<Vector3Int, bool> canEnPassant = (Vector3Int index) => enPassantableIndices.Contains(index) && GetPiece(index - up) == blackPawn;

        if (IsOnBoard(oneRight) && (IsBlack(GetPiece(oneRight)) || canEnPassant(oneRight)))
        {
            moves.Add(oneRight);
        }

        if (IsOnBoard(oneForward) && (IsBlack(GetPiece(oneForward)) || canEnPassant(oneForward)))
        {
            moves.Add(oneForward);
        }

        if (checkKingInCheck)
        {
            moves.RemoveWhere(move => IsWhiteKingInCheckAfterMove(pawnPosition, move, false));
        }

        return moves;
    }

    private HashSet<Vector3Int> GetPossibleMovesBlackPawnFiltered(int x, int y, int z, bool checkKingInCheck = true)
    {
        HashSet<Vector3Int> moves = new HashSet<Vector3Int>();
        Vector3Int pawnPosition = new Vector3Int(x, y, z);

        Vector3Int oneDown = pawnPosition - this.up;
        Vector3Int twoDown = pawnPosition - 2 * this.up;
        Vector3Int oneNRight = pawnPosition - this.forward;
        Vector3Int oneNForward = pawnPosition - this.right;

        if (IsOnBoard(oneDown) && GetPiece(oneDown) == empty)
        {
            if (IsOnBoard(twoDown) && GetPiece(twoDown) == empty && CanBlackPawnMoveTwoTiles(x, y, z))
            {
                moves.Add(twoDown);
            }
            moves.Add(oneDown);
        }

        System.Func<Vector3Int, bool> canEnPassant = (Vector3Int index) => enPassantableIndices.Contains(index) && GetPiece(index + up) == whitePawn;

        if (IsOnBoard(oneNRight) && (IsWhite(GetPiece(oneNRight)) || canEnPassant(oneNRight)))
        {
            moves.Add(oneNRight);
        }

        if (IsOnBoard(oneNForward) && (IsWhite(GetPiece(oneNForward)) || canEnPassant(oneNForward)))
        {
            moves.Add(oneNForward);
        }

        if (checkKingInCheck)
        {
            moves.RemoveWhere(move => IsBlackKingInCheckAfterMove(pawnPosition, move, false));
        }

        return moves;
    }

    private HashSet<Vector3Int> GetPossibleMovesKnightFiltered(int x, int y, int z, byte? color = null, bool checkKingInCheck = true)
    {
        HashSet<Vector3Int> moves = new HashSet<Vector3Int>();
        Vector3Int knightPosition = new Vector3Int(x, y, z);

        color ??= GetPieceColor(GetPiece(knightPosition));

        Vector3Int twoUpRight = knightPosition + 2 * this.up + this.right;
        Vector3Int twoUpLeft = knightPosition + 2 * this.up + this.forward;

        Vector3Int twoRightUp = knightPosition + 2 * this.right + this.up;
        Vector3Int twoRightDown = knightPosition + 2 * this.right - this.forward;

        Vector3Int twoLeftUp = knightPosition + 2 * this.forward + this.up;
        Vector3Int twoLeftDown = knightPosition + 2 * this.forward - this.right;

        // To make this less verbose one can initialize above vectors as relatives to knightPosition
        // and then use them to calculate the opposite vectors below
        Vector3Int oppositeTwoUpRight = knightPosition - 2 * this.up - this.forward;
        Vector3Int oppositeTwoUpLeft = knightPosition - 2 * this.up - this.right;

        Vector3Int oppositeTwoRightUp = knightPosition - 2 * this.right - this.up;
        Vector3Int oppositeTwoRightDown = knightPosition - 2 * this.right + this.forward;

        Vector3Int oppositeTwoLeftUp = knightPosition - 2 * this.forward - this.up;
        Vector3Int oppositeTwoLeftDown = knightPosition - 2 * this.forward + this.right;

        System.Func<byte, bool> IsSameColor = color == white ? this.IsWhite : this.IsBlack;

        foreach (Vector3Int move in new Vector3Int[] { twoUpRight, twoUpLeft, twoRightUp, twoRightDown, twoLeftUp, twoLeftDown,
            oppositeTwoUpRight, oppositeTwoUpLeft, oppositeTwoRightUp, oppositeTwoRightDown, oppositeTwoLeftUp, oppositeTwoLeftDown })
        {
            if (IsOnBoard(move) && !IsSameColor(GetPiece(move)))
            {
                moves.Add(move);
            }
        }

        if (checkKingInCheck)
        {
            System.Func<Vector3Int, Vector3Int, bool> IsKingInCheckAfterMove = color == white ? (x, y) => this.IsWhiteKingInCheckAfterMove(x, y, false) : (x, y) => this.IsBlackKingInCheckAfterMove(x, y, false);
            moves.RemoveWhere(move => IsKingInCheckAfterMove(knightPosition, move));
        }

        return moves;
    }

    private HashSet<Vector3Int> GetPossibleMovesBishopFiltered(int x, int y, int z, byte? color = null, bool checkKingInCheck = true)
    {
        HashSet<Vector3Int> moves = new HashSet<Vector3Int>();
        Vector3Int bishopPosition = new Vector3Int(x, y, z);

        color ??= GetPieceColor(GetPiece(bishopPosition));

        Vector3Int upRight = this.up + this.right;
        Vector3Int upLeft = this.up + this.forward;
        Vector3Int rightRight = this.right - this.forward;

        System.Func<byte, bool> IsSameColor = color == white ? this.IsWhite : this.IsBlack;

        foreach (Vector3Int direction in new Vector3Int[] { upRight, upLeft, rightRight, -upRight, -upLeft, -rightRight })
        {
            Vector3Int move = bishopPosition + direction;
            while (IsOnBoard(move) && GetPiece(move) == empty)
            {
                moves.Add(move);
                move += direction;
            }
            if (IsOnBoard(move) && !IsSameColor(GetPiece(move)))
            {
                moves.Add(move);
            }
        }

        if (checkKingInCheck)
        {
            System.Func<Vector3Int, Vector3Int, bool> IsKingInCheckAfterMove = color == white ? (x, y) => this.IsWhiteKingInCheckAfterMove(x, y, false) : (x, y) => this.IsBlackKingInCheckAfterMove(x, y, false);
            moves.RemoveWhere(move => IsKingInCheckAfterMove(bishopPosition, move));
        }

        return moves;
    }

    private HashSet<Vector3Int> GetPossibleMovesRookFiltered(int x, int y, int z, byte? color = null, bool checkKingInCheck = true)
    {
        HashSet<Vector3Int> moves = new HashSet<Vector3Int>();
        Vector3Int rookPosition = new Vector3Int(x, y, z);

        color ??= GetPieceColor(GetPiece(rookPosition));

        System.Func<byte, bool> IsSameColor = color == white ? this.IsWhite : this.IsBlack;

        foreach (Vector3Int direction in new Vector3Int[] { this.up, this.right, this.forward, -this.up, -this.right, -this.forward })
        {
            Vector3Int move = rookPosition + direction;
            while (IsOnBoard(move) && GetPiece(move) == empty)
            {
                moves.Add(move);
                move += direction;
            }
            if (IsOnBoard(move) && !IsSameColor(GetPiece(move)))
            {
                moves.Add(move);
            }
        }

        if (checkKingInCheck)
        {
            System.Func<Vector3Int, Vector3Int, bool> IsKingInCheckAfterMove = color == white ? (x, y) => this.IsWhiteKingInCheckAfterMove(x, y, false) : (x, y) => this.IsBlackKingInCheckAfterMove(x, y, false);
            moves.RemoveWhere(move => IsKingInCheckAfterMove(rookPosition, move));
        }

        return moves;
    }

    private HashSet<Vector3Int> GetPossibleMovesQueenFiltered(int x, int y, int z, byte? color = null, bool checkKingInCheck = true)
    {
        HashSet<Vector3Int> moves = GetPossibleMovesBishopFiltered(x, y, z);
        moves.UnionWith(GetPossibleMovesRookFiltered(x, y, z));
        return moves;
    }

    private HashSet<Vector3Int> GetPossibleMovesKingFiltered(int x, int y, int z, byte? color = null, bool checkKingInCheck = true)
    {
        HashSet<Vector3Int> moves = new HashSet<Vector3Int>();
        Vector3Int kingPosition = new Vector3Int(x, y, z);

        color ??= GetPieceColor(GetPiece(kingPosition));

        System.Func<byte, bool> IsSameColor = color == white ? this.IsWhite : this.IsBlack;

        foreach (Vector3Int direction in new Vector3Int[] { this.up, this.right, this.forward, this.up + this.right, this.up + this.forward, this.right - this.forward })
        {
            Vector3Int movePos = kingPosition + direction;
            Vector3Int moveNeg = kingPosition - direction;
            if (IsOnBoard(movePos) && !IsSameColor(GetPiece(movePos)))
            {
                moves.Add(movePos);
            }
            if (IsOnBoard(moveNeg) && !IsSameColor(GetPiece(moveNeg)))
            {
                moves.Add(moveNeg);
            }
        }

        if (checkKingInCheck)
        {
            System.Func<Vector3Int, bool> IsAttackedAfterMove = color == white ? x => this.IsAttackedByBlackAfterMove(x, kingPosition, x, false) : x => this.IsAttackedByWhiteAfterMove(x, kingPosition, x, false);
            moves.RemoveWhere(move => IsAttackedAfterMove(move));
        }

        return moves;
    }

    private bool CanWhitePawnMoveTwoTiles(int x, int y, int z)
    {
        return (x == 5 && y == 4 && z == 4) || (z == 4 && x - y == 1 && x < 5) || (y == 4 && x + z == 9 && 5 < x);
    }

    private bool CanBlackPawnMoveTwoTiles(int x, int y, int z)
    {
        return (x == 5 && y == 6 && z == 6) || (z == 6 && y - x == 1 && 5 < x) || (y == 6 && x + z == 11 && x < 5);
    }

    private bool IsAttackedByBlack(int x, int y, int z, bool checkKingInCheck = true)
    {
        HashSet<Vector3Int> possiblePawnMoves = GetPossibleMovesWhitePawnFiltered(x, y, z, checkKingInCheck);
        foreach (Vector3Int move in possiblePawnMoves)
        {
            if (GetPiece(move) == blackPawn)
            {
                return true;
            }
        }

        HashSet<Vector3Int> possibleKnightMoves = GetPossibleMovesKnightFiltered(x, y, z, white, checkKingInCheck);
        foreach (Vector3Int move in possibleKnightMoves)
        {
            if (GetPiece(move) == blackKnight)
            {
                return true;
            }
        }

        HashSet<Vector3Int> possibleBishopMoves = GetPossibleMovesBishopFiltered(x, y, z, white, checkKingInCheck);
        foreach (Vector3Int move in possibleBishopMoves)
        {
            if (GetPiece(move) == blackBishop || GetPiece(move) == blackQueen)
            {
                return true;
            }
        }

        HashSet<Vector3Int> possibleRookMoves = GetPossibleMovesRookFiltered(x, y, z, white, checkKingInCheck);
        foreach (Vector3Int move in possibleRookMoves)
        {
            if (GetPiece(move) == blackRook || GetPiece(move) == blackQueen)
            {
                return true;
            }
        }

        HashSet<Vector3Int> possibleKingMoves = GetPossibleMovesKingFiltered(x, y, z, white, checkKingInCheck);
        foreach (Vector3Int move in possibleKingMoves)
        {
            if (GetPiece(move) == blackKing)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsPromoteTileForWhite(Vector3Int index)
    {
        index -= Vector3Int.one * 5;
        return (index.x + index.z == 5) || (index.y - index.x == 5);
    }

    private bool IsPromoteTileForBlack(Vector3Int index)
    {
        index -= Vector3Int.one * 5;
        return (index.x + index.z == -5) || (index.y - index.x == -5);
    }

    private bool IsAttackedByBlack(Vector3Int index, bool checkKingInCheck = true)
    {
        return IsAttackedByBlack(index.x, index.y, index.z, checkKingInCheck);
    }

    private bool IsAttackedByBlackAfterMove(Vector3Int index, Vector3Int from, Vector3Int to, bool checkKingInCheck = true)
    {
        byte piece = GetPiece(from);
        byte removedPiece = GetPiece(to);

        board[to.x, to.y, to.z] = piece;
        board[from.x, from.y, from.z] = empty;

        bool isAttacked = IsAttackedByBlack(index, checkKingInCheck);

        board[from.x, from.y, from.z] = piece;
        board[to.x, to.y, to.z] = removedPiece;

        return isAttacked;
    }

    private bool IsWhiteKingInCheck(bool checkKingInCheck = true)
    {
        return IsAttackedByBlack(whiteKingPosition, checkKingInCheck);
    }

    private bool IsWhiteKingInCheckAfterMove(Vector3Int from, Vector3Int to, bool checkKingInCheck = true)
    {
        return IsAttackedByBlackAfterMove(whiteKingPosition, from, to, checkKingInCheck);
    }

    private bool IsAttackedByWhite(int x, int y, int z, bool checkKingInCheck = true)
    {
        HashSet<Vector3Int> possiblePawnMoves = GetPossibleMovesBlackPawnFiltered(x, y, z, checkKingInCheck);
        foreach (Vector3Int move in possiblePawnMoves)
        {
            if (GetPiece(move) == whitePawn)
            {
                return true;
            }
        }

        HashSet<Vector3Int> possibleKnightMoves = GetPossibleMovesKnightFiltered(x, y, z, black, checkKingInCheck);
        foreach (Vector3Int move in possibleKnightMoves)
        {
            if (GetPiece(move) == whiteKnight)
            {
                return true;
            }
        }

        HashSet<Vector3Int> possibleBishopMoves = GetPossibleMovesBishopFiltered(x, y, z, black, checkKingInCheck);
        foreach (Vector3Int move in possibleBishopMoves)
        {
            if (GetPiece(move) == whiteBishop || GetPiece(move) == whiteQueen)
            {
                return true;
            }
        }

        HashSet<Vector3Int> possibleRookMoves = GetPossibleMovesRookFiltered(x, y, z, black, checkKingInCheck);
        foreach (Vector3Int move in possibleRookMoves)
        {
            if (GetPiece(move) == whiteRook || GetPiece(move) == whiteQueen)
            {
                return true;
            }
        }

        HashSet<Vector3Int> possibleKingMoves = GetPossibleMovesKingFiltered(x, y, z, black, checkKingInCheck);
        foreach (Vector3Int move in possibleKingMoves)
        {
            if (GetPiece(move) == whiteKing)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsAttackedByWhite(Vector3Int index, bool checkKingInCheck = true)
    {
        return IsAttackedByWhite(index.x, index.y, index.z, checkKingInCheck);
    }

    private bool IsAttackedByWhiteAfterMove(Vector3Int index, Vector3Int from, Vector3Int to, bool checkKingInCheck = true)
    {
        byte piece = GetPiece(from);
        byte removedPiece = GetPiece(to);

        board[to.x, to.y, to.z] = piece;
        board[from.x, from.y, from.z] = empty;

        bool isAttacked = IsAttackedByWhite(index, checkKingInCheck);

        board[from.x, from.y, from.z] = piece;
        board[to.x, to.y, to.z] = removedPiece;

        return isAttacked;
    }

    private bool IsBlackKingInCheck(bool checkKingInCheck = true)
    {
        return IsAttackedByWhite(blackKingPosition, checkKingInCheck);
    }

    private bool IsBlackKingInCheckAfterMove(Vector3Int from, Vector3Int to, bool checkKingInCheck = true)
    {
        return IsAttackedByWhiteAfterMove(blackKingPosition, from, to, checkKingInCheck);
    }

    private bool HasWhiteWon()
    {
        return IsBlackKingInCheck() && GetPossibleMovesKingFiltered(blackKingPosition.x, blackKingPosition.y, blackKingPosition.z).Count == 0;
    }

    private bool HasBlackWon()
    {
        return IsWhiteKingInCheck() && GetPossibleMovesKingFiltered(whiteKingPosition.x, whiteKingPosition.y, whiteKingPosition.z).Count == 0;
    }

    private byte GetPieceColor(byte piece)
    {
        return (byte)(piece & 0b0000_0001);
    }

    private bool IsWhite(byte piece)
    {
        return piece != empty && GetPieceColor(piece) == 0b0000_0000;
    }

    private bool IsBlack(byte piece)
    {
        return GetPieceColor(piece) == 0b0000_0001;
    }

    private byte GetPiece(int x, int y, int z)
    {
        return board[x, y, z];
    }

    private byte GetPiece(Vector3Int index)
    {
        return board[index.x, index.y, index.z];
    }

    private bool IsOnBoard(int x, int y, int z)
    {
        return x >= 0 && x < 11 && y >= 0 && y < 11 && z >= 0 && z < 11;
    }

    private bool IsOnBoard(Vector3Int position)
    {
        return Util.CheckEveryAxis(position, (float axis) => axis >= 0 && axis < 11);
    }

    private int GetTileColorIndex(int x, int y, int z)
    {
        int index = (Mathf.Min(y, z) - Mathf.Abs(y - z) - 1) % tileColors.Length;
        if (index < 0)
        {
            index += tileColors.Length;
        }
        return tileColors.Length - 1 - index;
    }

    private Color GetTileColor(int x, int y, int z)
    {
        return tileColors[GetTileColorIndex(x, y, z)];
    }

    private Color GetTileColor(Vector3Int index)
    {
        return GetTileColor(index.x, index.y, index.z);
    }

    private void UpdateLocalGameVisuals()
    {
        foreach (Vector3Int index in everyValidTileIndex)
        {
            if (IsBlack(GetPiece(index)))
            {
                GetTile(index).FlipPiece();
            }
        }
    }

    [ContextMenu("ToggleLocalGame")]
    public void ToggleLocalGame()
    {
        UpdateLocalGameVisuals();
        localGame = !localGame;
    }

    public void CancelPromotion()
    {
        UndoLastMove();
        whiteTurn = !whiteTurn;
    }

    public void Promote(Vector3Int index, byte pieceToPromoteTo)
    {
        byte promotedPiece = GetPiece(index);
        if (promotedPiece == whitePawn || promotedPiece == blackPawn)
        {
            board[index.x, index.y, index.z] = pieceToPromoteTo;
            GetTile(index).SetPieceSprite(GetPieceSprite(pieceToPromoteTo));
            promotionHappened.Add(pointInHistory);
            whiteTurn = !whiteTurn;
        }
        else
        {
            CancelPromotion();
        }
    }
}
