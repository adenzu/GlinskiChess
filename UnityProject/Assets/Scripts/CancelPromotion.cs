using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CancelPromotion : MonoBehaviour
{
    public PromotionUI promotionUI;

    private void OnMouseDown()
    {
        promotionUI.CancelPromotion();
    }
}
