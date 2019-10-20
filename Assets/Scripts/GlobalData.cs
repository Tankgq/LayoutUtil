using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
		if(string.IsNullOrWhiteSpace(name)) return null;
		return string.IsNullOrWhiteSpace(CurrentModule) ? null : ModuleDic[CurrentModule].Find(element => element.Name.Equals(name));
	}

	public static string CurrentModule;

	public static string CurrentFilePath = null;

	public static readonly Vector2 OriginPoint = new Vector2(32, -32);
	public static readonly Vector2 DefaultSize = new Vector2(64, 64);
	public const string DefaultName = "DisplayObject";
	public const float MinFloat = -100000000;
	public const float MaxFloat = 100000000;

	private const int TargetFrameRate = 60;

	public static bool IsDragGui = false;

	public const int QuickTipMaxCount = 9;
	public const float QuickTipDuration = 3.0f;

	public static readonly Dictionary<string, bool> ModifyDic = new Dictionary<string, bool>();

	public static int ModifyCount {
		get { return ModifyDic.Count(pair => pair.Value); }
		set {
			if(value != 0) return;
			List<string> modifyKeys = ModifyDic.Select(pair => pair.Key).ToList();
			foreach(string modifyKey in modifyKeys) {
				ModifyDic[modifyKey] = false;
			}
		}
	}

	public static Shader DefaultShader;

	public static Camera MainCamera;

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
	public static GameObject HierarchyContainer;
	public static GameObject QuickTipContainer;

//	public static ContainerManager ContainerManager;
	public static HierarchyManager HierarchyManager;
	public static RectTransform ContainerRect;
	public static RectTransform RootCanvasRect;
	public static Slider ScaleSlider;

	private void Awake() {
		Application.targetFrameRate = TargetFrameRate;
		DefaultShader = Shader.Find("UI/Default");
		MainCamera = Camera.main;
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
		HierarchyContainer = GameObject.FindGameObjectWithTag("HierarchyContainer");
		QuickTipContainer = GameObject.FindGameObjectWithTag("QuickTipContainer");
//		ContainerManager = DisplayObjectContainer.GetComponent<ContainerManager>();
		HierarchyManager = HierarchyContainer.GetComponent<HierarchyManager>();
		ContainerRect = DisplayObjectContainer.GetComponent<RectTransform>();
		RootCanvasRect = RootCanvas.GetComponent<RectTransform>();
		ScaleSlider = GameObject.FindGameObjectWithTag("ScaleSlider").GetComponent<Slider>();
	}
}
