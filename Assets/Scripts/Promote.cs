using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Promote : MonoBehaviour
{
    public Vector3Int index;
    public byte pieceToPromoteTo;

    private void OnMouseDown()
    {
        GameObject.Find("Board").GetComponent<GlinskiChess>().Promote(index, pieceToPromoteTo);
        Destroy(transform.parent.gameObject);
    }
}
