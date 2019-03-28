using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalData
{
    public static Dictionary<int, Transform> CurrentSelectDisplayObjects = new Dictionary<int, Transform>();

    public static List<Transform> DisplayObjects = new List<Transform>();
    public static Dictionary<int, string> DisplayObjectPaths = new Dictionary<int, string>();

    public static readonly Vector2 OriginPoint = new Vector2(32, 32);
    public static readonly Vector2 DefaultSize = new Vector2(64, 64);
    public static readonly float MinFloat = -100000000;
}
