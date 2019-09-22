﻿using System.Collections.Generic;
using UnityEngine;

public class GlobalData : MonoBehaviour {
	public const string ProductName = "LayoutUtil";

	public const int CloseValue = 10;
	public const int AlignExtensionValue = 16;
	public const int AlignLineThickness = 1;

	public const string GlobalObservable = "Observable";
	public static int UniqueId = 0;

	public static readonly SortedDictionary<string, Transform> CurrentSelectDisplayObjectDic =
			new SortedDictionary<string, Transform>();

	public static readonly List<Element> CurrentCopyDisplayObjects = new List<Element>();

	public static readonly List<Transform> CurrentDisplayObjects = new List<Transform>();
	public static readonly Dictionary<string, Transform> CurrentDisplayObjectDic = new Dictionary<string, Transform>();

	public static readonly Dictionary<string, string> DisplayObjectPathDic = new Dictionary<string, string>();

	public static readonly List<string> Modules = new List<string>();
	public static readonly Dictionary<string, List<Element>> ModuleDic = new Dictionary<string, List<Element>>();

	public static readonly Dictionary<string, List<Element>> CacheModuleDic = new Dictionary<string, List<Element>>();

	public static Element GetElement(string name) {
		if(string.IsNullOrEmpty(name)) return null;
		return string.IsNullOrWhiteSpace(CurrentModule)
					   ? null
					   : ModuleDic[CurrentModule].Find(element => element.Name.Equals(name));
	}

	public static string PreviousModule;
	private static string _currentModule;

	public static string CurrentModule {
		get { return _currentModule; }
		set {
			PreviousModule = _currentModule;
			_currentModule = value;
		}
	}

	public static string PreviousFilePath = null;
	private static string _currentFilePath = null;

	public static string CurrentFilePath {
		get { return _currentFilePath; }
		set {
			PreviousFilePath = _currentFilePath;
			_currentFilePath = value;
		}
	}

	public static readonly Vector2 OriginPoint = new Vector2(32, -32);
	public static readonly Vector2 DefaultSize = new Vector2(64, 64);
	public const string DefaultName = "DisplayObject";
	public const float MinFloat = -100000000;
	public const float MaxFloat = 100000000;

	public static bool IsDragGui = false;

	public const int QuickTipMaxCount = 9;
	public const float QuickTipDuration = 2.0f;

	public static int ModifyCount = 0;

	public static Shader DefaultShader;

	public static GameObject DisplayObjectItemPrefab;
	public static GameObject DisplayObjectPrefab;
	public static GameObject ModuleItemPrefab;
	public static GameObject DialogPrefab;
	public static GameObject TipPrefab;
	public static GameObject SelectPrefab;
	public static GameObject LinePrefab;
	public static GameObject QuickTipPrefab;

	public static GameObject RootCanvas;
	public static GameObject DisplayObjectContainer;
	public static ContainerManager ContainerManager;
	public static GameObject HierarchyContainer;

	public static GameObject QuickTipContainer;

	private void Awake() {
		DefaultShader = Shader.Find("UI/Default");
		DisplayObjectItemPrefab = Resources.Load<GameObject>("Prefabs/DisplayObjectItem");
		DisplayObjectPrefab = Resources.Load<GameObject>("Prefabs/DisplayObject");
		ModuleItemPrefab = Resources.Load<GameObject>("Prefabs/ModuleItem");
		DialogPrefab = Resources.Load<GameObject>("Prefabs/Dialog");
		TipPrefab = Resources.Load<GameObject>("Prefabs/Tip");
		SelectPrefab = Resources.Load<GameObject>("Prefabs/Select-Rect");
		LinePrefab = Resources.Load<GameObject>("Prefabs/Line");
		QuickTipPrefab = Resources.Load<GameObject>("Prefabs/Quick-Tip");
		RootCanvas = GameObject.FindGameObjectWithTag("RootCanvas");
		DisplayObjectContainer = GameObject.FindGameObjectWithTag("DisplayObjectContainer");
		ContainerManager = DisplayObjectContainer.GetComponent<ContainerManager>();
		HierarchyContainer = GameObject.FindGameObjectWithTag("HierarchyContainer");
		QuickTipContainer = GameObject.FindGameObjectWithTag("QuickTipContainer");
	}
}
