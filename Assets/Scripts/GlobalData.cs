using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class GlobalData : MonoBehaviour
{
    public static int NameId = 0;
    public static Dictionary<int, Transform> CurrentSelectDisplayObjects = new Dictionary<int, Transform>();

    public static void AddCurrentSelectObject(Transform displayObject)
    {
        if (!displayObject) return;
        Observable.Timer(TimeSpan.Zero)
            .Subscribe(_ => CurrentSelectDisplayObjects[displayObject.GetInstanceID()] = displayObject);
    }

    public static List<Transform> DisplayObjects = new List<Transform>();
    public static Dictionary<int, string> DisplayObjectPaths = new Dictionary<int, string>();

    public static Dictionary<string, DisplayObject>  

    public static readonly Vector2 OriginPoint = new Vector2(32, 32);
    public static readonly Vector2 OriginPointPosition = new Vector2(OriginPoint.x, - OriginPoint.y);
    public static readonly Vector2 DefaultSize = new Vector2(64, 64);
    public const string DefaultName = "DisplayObject";
    public const float MinFloat = -100000000;

    public static GameObject DisplayObjectItemPrefab;
    public static GameObject DisplayObjectPrefab;
    public static GameObject ModuleItemPrefab;
    public static GameObject DialogPrefab;

    public static GameObject RootCanvas;

    private void Awake()
    {
        DisplayObjectItemPrefab = Resources.Load<GameObject>("Prefabs/DisplayObjectItem");
        DisplayObjectPrefab = Resources.Load<GameObject>("Prefabs/DisplayObject");
        ModuleItemPrefab = Resources.Load<GameObject>("Prefabs/ModuleItem");
        DialogPrefab = Resources.Load<GameObject>("Prefabs/Dialog");
        RootCanvas = GameObject.FindGameObjectWithTag("Canvas");
    }
}
