using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalData
{
    public static Dictionary<int, Transform> curSelectDisplayObjects = new Dictionary<int, Transform>();

    public static readonly Vector2 OriginPoint = new Vector2(32, 32);
}
