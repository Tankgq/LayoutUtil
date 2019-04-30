using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace Assets.Scripts
{
	public class GlobalData : MonoBehaviour
	{
		public const int CLOSE_VALUE = 10;
		public const int ALIGN_EXTENSION_VALUE = 16;
		public const int ALIGN_LINE_THICKNESS = 1;

		public static readonly string GlobalObservable = "Observable";
		public static int UniqueId = 0;

		public static readonly Dictionary<string, Transform> CurrentSelectDisplayObjectDic = new Dictionary<string, Transform>();

		public static readonly List<Element> CurrentCopyDisplayObjects = new List<Element>();

		public static void AddCurrentSelectObject(string currentModule, Transform displayObject)
		{
			if (!displayObject) return;
			Observable.Timer(TimeSpan.Zero)
				.Subscribe(_ =>
				{
					if (string.IsNullOrEmpty(currentModule) || !currentModule.Equals(GlobalData.CurrentModule))
						return;
					CurrentSelectDisplayObjectDic[displayObject.name] = displayObject;
				});
		}

		public static readonly List<Transform> CurrentDisplayObjects = new List<Transform>();
		public static readonly Dictionary<string, Transform> CurrentDisplayObjectDic = new Dictionary<string, Transform>();

		public static readonly Dictionary<string, string> DisplayObjectPathDic = new Dictionary<string, string>();

		public static readonly List<string> ModuleNames = new List<string>();
		public static readonly Dictionary<string, List<Element>> Modules = new Dictionary<string, List<Element>>();
		public static Element GetDisplayObjectData(string name)
		{
			if (string.IsNullOrEmpty(name)) return null;
			if (string.IsNullOrEmpty(CurrentModule)) return null;
			return Modules[CurrentModule].Find(element => element.Name.Equals(name));
		}

		public static string CurrentModule = null;

		public static readonly Vector2 OriginPoint = new Vector2(32, -32);
		public static readonly Vector2 DefaultSize = new Vector2(64, 64);
		public const string DefaultName = "DisplayObject";
		public const float MinFloat = -100000000;
		public const float MaxFloat = 100000000;

		public static bool IsDragGui = false;

		public static Shader DefaultShader;

		public static GameObject DisplayObjectItemPrefab;
		public static GameObject DisplayObjectPrefab;
		public static GameObject ModuleItemPrefab;
		public static GameObject DialogPrefab;
		public static GameObject TipPrefab;
		public static GameObject SelectPrefab;
		public static GameObject LinePrefab;

		public static GameObject RootCanvas;
		public static GameObject DisplayObjectContainer;
		public static ContainerManager ContainerManager;
		public static GameObject HierarchyContainer;

		private void Awake()
		{
			DefaultShader = Shader.Find("UI/Default");
			DisplayObjectItemPrefab = Resources.Load<GameObject>("Prefabs/DisplayObjectItem2");
			DisplayObjectPrefab = Resources.Load<GameObject>("Prefabs/DisplayObject");
			ModuleItemPrefab = Resources.Load<GameObject>("Prefabs/ModuleItem");
			DialogPrefab = Resources.Load<GameObject>("Prefabs/Dialog");
			TipPrefab = Resources.Load<GameObject>("Prefabs/Tip");
			SelectPrefab = Resources.Load<GameObject>("Prefabs/Select-Rect");
			LinePrefab = Resources.Load<GameObject>("Prefabs/Line");
			RootCanvas = GameObject.FindGameObjectWithTag("RootCanvas");
			DisplayObjectContainer = GameObject.FindGameObjectWithTag("DisplayObjectContainer");
			ContainerManager = DisplayObjectContainer.GetComponent<ContainerManager>();
			HierarchyContainer = GameObject.FindGameObjectWithTag("HierarchyContainer");
		}
	}
}
