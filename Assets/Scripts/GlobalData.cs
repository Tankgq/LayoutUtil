using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class GlobalData : MonoBehaviour
{
    public static string GlobalObservable = "Observable";
    public static int UniqueId = 0;
    public static Dictionary<string, Transform> CurrentSelectDisplayObjectDic = new Dictionary<string, Transform>();

    public static void AddCurrentSelectObject(string currentModule, Transform displayObject)
    {
        if (!displayObject) return;
        Observable.Timer(TimeSpan.Zero)
            .Subscribe(_ => CurrentSelectDisplayObjectDic[$"{currentModule}_{displayObject.name}"] = displayObject);
    }

    public static List<Transform> CurrentDisplayObjects = null;
    public static Dictionary<string, string> DisplayObjectPathDic = new Dictionary<string, string>();

    public static Dictionary<string, Transform> DisplayObjectNameDic = new Dictionary<string, Transform>();
    public static Dictionary<string, DisplayObject> DisplayObjectDataDic = new Dictionary<string, DisplayObject>();

    public static List<string> ModuleNames = new List<string>();
    public static Dictionary<string, List<Transform>> Modules = new Dictionary<string, List<Transform>>();

    public static string CurrentModule = null;

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
