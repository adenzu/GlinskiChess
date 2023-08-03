using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    public static bool CheckEveryAxis(Vector3 vector, System.Func<float, bool> Predicate)
    {
        return Predicate(vector.x) && Predicate(vector.y) && Predicate(vector.z);
    }

    public static Vector3 GetMousePosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = 10;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);
        return mousePosition;
    }
}
