using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public static class DisplayObjectUtil {
	private static readonly Dictionary<string, Material> MaterialDic = new Dictionary<string, Material>();
	private static readonly Dictionary<string, Vector2> SizeDic = new Dictionary<string, Vector2>();

	private static readonly List<Transform> DisplayObjectPool = new List<Transform>();

	private static Transform GetDisplayObject() {
		int length = DisplayObjectPool.Count;
		if(length == 0) return Object.Instantiate(GlobalData.DisplayObjectPrefab.transform, GlobalData.DisplayObjectContainer.transform);
		Transform result = DisplayObjectPool[length - 1];
		DisplayObjectPool.RemoveAt(length - 1);
		return result;
	}

	private static void RecycleDisplayObject(Transform displayObject) {
		if(! displayObject) return;
		DisplayObjectManager.DeSelectDisplayObject(displayObject);
		displayObject.GetComponentInChildren<Toggle>().isOn = false;
		displayObject.SetParent(null);
		DisplayObjectPool.Add(displayObject);
	}

	public static void RemoveAllDisplayObjectBehavior() {
		int count = GlobalData.CurrentDisplayObjects.Count;
		for(int idx = 0; idx < count; ++ idx) RecycleDisplayObject(GlobalData.CurrentDisplayObjects[idx]);
		GlobalData.CurrentDisplayObjects.Clear();
		GlobalData.CurrentDisplayObjectDic.Clear();
		GlobalData.CurrentCopyDisplayObjects.Clear();
		GlobalData.CurrentSelectDisplayObjectDic.Clear();
	}

	public static void AddAllDisplayObjectBehavior() {
		if(string.IsNullOrWhiteSpace(GlobalData.CurrentModule)) return;
		List<Element> elements = GlobalData.ModuleDic[GlobalData.CurrentModule];
		int count = elements.Count;
		for(int idx = 0; idx < count; ++ idx) {
			AddDisplayObjectBehavior(GlobalData.CurrentModule, elements[idx]);
		}
	}

	public static void AddDisplayObjectBehavior(string moduleName, Element element, string imageUrl = null) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! moduleName.Equals(GlobalData.CurrentModule)) return;
		if(element == null) return;
		GlobalData.ModuleDic[moduleName].Add(element);
		Transform displayObject = GetDisplayObject();
		displayObject.SetParent(GlobalData.DisplayObjectContainer.transform);
		element.InvConvertTo(displayObject);
		GlobalData.CurrentDisplayObjects.Add(displayObject);
		GlobalData.CurrentDisplayObjectDic[element.Name] = displayObject;
		LoadImageBehavior(GlobalData.CurrentModule, element.Name, imageUrl);
		displayObject.GetComponent<RectTransform>().localScale = Vector3.one;
	}

	public static void RemoveDisplayObjectBehavior(string moduleName, string elementName) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! moduleName.Equals(GlobalData.CurrentModule)) return;
		if(string.IsNullOrWhiteSpace(elementName)) return;
		Transform displayObject = GlobalData.CurrentDisplayObjectDic[elementName];
		if(displayObject) RecycleDisplayObject(displayObject);
		int idx = GlobalData.CurrentDisplayObjects.FindIndex(0, element => elementName.Equals(element.name));
		if(idx != -1) GlobalData.CurrentDisplayObjects.RemoveAt(idx);
		List<Element> elements = GlobalData.ModuleDic[GlobalData.CurrentModule];
		idx = elements.FindIndex(0, element => elementName.Equals(element.Name));
		if(idx != -1) elements.RemoveAt(idx);
	}

	public static void RemoveDisplayObjectsBehavior(string moduleName, List<string> elements) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! moduleName.Equals(GlobalData.CurrentModule)) return;
		if(elements == null || elements.Count == 0) return;
		foreach(string elementName in elements)
			RemoveDisplayObjectBehavior(moduleName, elementName);
	}
	
	public static void RemoveDisplayObjectsBehavior(string moduleName, List<Element> elements) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! moduleName.Equals(GlobalData.CurrentModule)) return;
		if(elements == null || elements.Count == 0) return;
		foreach(Element element in elements)
			RemoveDisplayObjectBehavior(moduleName, element.Name);
	}

	public static void AddDisplayObjectsBehavior(string moduleName, List<Element> elements) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! moduleName.Equals(GlobalData.CurrentModule)) return;
		if(elements == null || elements.Count == 0) return;
		foreach(Element element in elements)
			AddDisplayObjectBehavior(moduleName, element);
	}

	public static void LoadImageBehavior(string moduleName, string elementName, string imageUrl = null) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! moduleName.Equals(GlobalData.CurrentModule)) return;
		if(string.IsNullOrWhiteSpace(elementName)) return;
		Transform displayObject = GlobalData.CurrentDisplayObjectDic[elementName];
		if(! displayObject) return;
		Image image = displayObject.GetComponent<Image>();
		if(! image) return;
		string displayKey = $"{moduleName}_{elementName}";
		if(imageUrl == null) GlobalData.DisplayObjectPathDic.TryGetValue(displayKey, out imageUrl);
		Material material = null;
		if(! string.IsNullOrWhiteSpace(imageUrl)) MaterialDic.TryGetValue(imageUrl, out material);
		if(material != null) {
			GlobalData.DisplayObjectPathDic[displayKey] = imageUrl;
			image.material = material;
			image.color = Color.white;
		} else {
			GlobalData.DisplayObjectPathDic[displayKey] = null;
			image.color = Color.clear;
		}
	}

	public static void RemoveImageBehavior(string moduleName, string elementName) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! moduleName.Equals(GlobalData.CurrentModule)) return;
		if(string.IsNullOrWhiteSpace(elementName)) return;
		Transform displayObject = GlobalData.CurrentDisplayObjectDic[elementName];
		if(! displayObject) return;
		Image image = displayObject.GetComponent<Image>();
		if(! image) return;
		image.color = Color.clear;
	}

	public static Transform AddDisplayObject(string imageUrl, Vector2 pos, Vector2 size, string elementName = null) {
		if(string.IsNullOrEmpty(GlobalData.CurrentModule)) {
			if(GlobalData.ModuleDic.Count == 0) {
				DialogManager.ShowInfo("请先创建一个 module", 320);
				return null;
			}

			DialogManager.ShowInfo("请先打开一个 module", 320);
			return null;
		}

		if(! string.IsNullOrWhiteSpace(imageUrl)) {
			Material material;
			MaterialDic.TryGetValue(imageUrl, out material);
			if(material == null) {
				Texture2D texture = Utils.LoadTexture2DbyIo(imageUrl);
				material = new Material(GlobalData.DefaultShader) {mainTexture = texture};
				MaterialDic[imageUrl] = material;
				SizeDic[imageUrl] = new Vector2(texture.width, texture.height);
			}
			if(size == Vector2.zero) {
				if(! SizeDic.ContainsKey(imageUrl)) SizeDic[imageUrl] = GlobalData.DefaultSize;
				size = SizeDic[imageUrl];
			}
		}
		Transform displayObject = null;
		// 按住 Alt 导入图片到 DisplayObject 中, 优先导入当前选择 displayObject
		if(! string.IsNullOrEmpty(imageUrl) && KeyboardEventManager.GetAlt()) {
			foreach(var pair in GlobalData.CurrentSelectDisplayObjectDic.Where(pair => Utils.IsPointOverTransform(pair.Value))) {
				displayObject = pair.Value;
				break;
			}
			int length = GlobalData.CurrentDisplayObjects.Count;
			for(int idx = length - 1; idx >= 0; -- idx) {
				if(! Utils.IsPointOverTransform(GlobalData.CurrentDisplayObjects[idx])) continue;
				displayObject = GlobalData.CurrentDisplayObjects[idx];
				break;
			}
			if(displayObject == null) return null;
			HistoryManager.Do(BehaviorFactory.GetLoadImageToDisplayObjectBehavior(GlobalData.CurrentModule, displayObject.name, imageUrl));
			return displayObject;
		}

		elementName = string.IsNullOrWhiteSpace(elementName)
							  ? string.IsNullOrEmpty(imageUrl) ? GlobalData.DefaultName + ++ GlobalData.UniqueId : Utils.GetFileNameInPath(imageUrl)
							  : elementName;
		pos = Element.ConvertTo(pos);
		if(GlobalData.CurrentDisplayObjectDic.ContainsKey(elementName)) elementName += ++ GlobalData.UniqueId;
		
		HistoryManager.Do(BehaviorFactory.GetAddDisplayObjectBehavior(GlobalData.CurrentModule, elementName, imageUrl, pos, size));
		GlobalData.CurrentDisplayObjectDic.TryGetValue(elementName, out displayObject);
		return displayObject;
	}

	public static void RemoveSelectedDisplayObject() {
		int count = GlobalData.CurrentSelectDisplayObjectDic.Count;
		if(count == 0) {
			DialogManager.ShowInfo("请先选择要删除的对象");
			return;
		}
		
		new Action<string>(moduleName => {
			List<Element> elements = GlobalData.CurrentSelectDisplayObjectDic.Select(pair => GlobalData.GetElement(pair.Key)).ToList();
			HistoryManager.Do(new Behavior(isRedo => {
											   foreach(Element element in elements) RemoveDisplayObjectBehavior(moduleName, element.Name, 1);
										   },
										   isReUndo => {
											   foreach(Element element in elements) {
												   AddDisplayObjectBehavior(moduleName, element, null, -1);
												   Transform displayObject = GlobalData.CurrentDisplayObjectDic[element.Name];
												   GlobalData.CurrentSelectDisplayObjectDic.Add(element.Name, displayObject);
											   }
										   }));
		})(GlobalData.CurrentModule);
	}

	public static void MoveCurrentSelectDisplayObjectUp() {
		if(GlobalData.CurrentSelectDisplayObjectDic.Count != 1) return;
		string displayObjectName = GlobalData.CurrentSelectDisplayObjectDic.First().Key;
		int idx = GlobalData.CurrentDisplayObjects.FindIndex(element => element.name.Equals(displayObjectName));
		if(idx <= 0 || idx >= GlobalData.CurrentDisplayObjects.Count) return;
		Transform tmp = GlobalData.CurrentDisplayObjects[idx];
		GlobalData.CurrentDisplayObjects[idx] = GlobalData.CurrentDisplayObjects[idx - 1];
		GlobalData.CurrentDisplayObjects[idx - 1] = tmp;
		int siblingIndex = tmp.GetSiblingIndex();
		tmp.SetSiblingIndex(siblingIndex - 1);
		List<Element> displayObjectDataList = GlobalData.ModuleDic[GlobalData.CurrentModule];
		Element tmp2 = displayObjectDataList[idx];
		displayObjectDataList[idx] = displayObjectDataList[idx - 1];
		displayObjectDataList[idx - 1] = tmp2;
	}

	public static void MoveCurrentSelectDisplayObjectDown() {
		if(GlobalData.CurrentSelectDisplayObjectDic.Count != 1) return;
		string displayObjectKey = GlobalData.CurrentSelectDisplayObjectDic.First().Key;
		string displayObjectName = Utils.GetDisplayObjectName(displayObjectKey);
		int idx = GlobalData.CurrentDisplayObjects.FindIndex(element => element.name.Equals(displayObjectName));
		if(idx < 0 || idx >= GlobalData.CurrentDisplayObjects.Count - 1) return;
		Transform tmp = GlobalData.CurrentDisplayObjects[idx];
		GlobalData.CurrentDisplayObjects[idx] = GlobalData.CurrentDisplayObjects[idx + 1];
		GlobalData.CurrentDisplayObjects[idx + 1] = tmp;
		int siblingIndex = tmp.GetSiblingIndex();
		tmp.SetSiblingIndex(siblingIndex + 1);
		List<Element> displayObjectDataList = GlobalData.ModuleDic[GlobalData.CurrentModule];
		Element tmp2 = displayObjectDataList[idx];
		displayObjectDataList[idx] = displayObjectDataList[idx + 1];
		displayObjectDataList[idx + 1] = tmp2;
	}

	public static Rectangle GetMinRectangleContainsDisplayObjects(IReadOnlyList<Element> displayObjects) {
		if(displayObjects == null || displayObjects.Count == 0) return null;
		Rectangle rect = new Rectangle();
		int count = displayObjects.Count;
		for(int idx = 0; idx < count; ++ idx) {
			rect.Left = Math.Min(rect.Left, displayObjects[idx].Left);
			rect.Right = Math.Max(rect.Right, displayObjects[idx].Right);
			rect.Top = Math.Min(rect.Top, displayObjects[idx].Top);
			rect.Bottom = Math.Max(rect.Bottom, displayObjects[idx].Bottom);
		}

		return rect;
	}

	public static AlignInfo GetAlignLine(Transform displayObject) {
		Element displayObjectData = GlobalData.GetElement(displayObject.name);
		if(displayObjectData == null) return null;
		RectTransform rt = GlobalData.DisplayObjectContainer.GetComponent<RectTransform>();
		float closeValue = GlobalData.CloseValue / rt.localScale.x;
		const float lineThickness = GlobalData.AlignLineThickness; // * rt.localScale.x;
		AlignInfo alignInfo = new AlignInfo(displayObjectData, closeValue, lineThickness);
		List<Element> elements = GlobalData.ModuleDic[GlobalData.CurrentModule];
		int count = elements.Count;
		for(int idx = 0; idx < count; ++ idx) {
			if(displayObjectData.Name.Equals(elements[idx].Name)) continue;
			if(GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(elements[idx].Name)) continue;
			alignInfo.Merge(elements[idx]);
		}

		return alignInfo;
	}

	public static void CopySelectDisplayObjects() {
		if(GlobalData.CurrentSelectDisplayObjectDic.Count == 0) return;
		GlobalData.CurrentCopyDisplayObjects.Clear();
		int count = GlobalData.CurrentDisplayObjects.Count;
		for(int idx = 0; idx < count; ++ idx) {
			Transform displayObject = GlobalData.CurrentDisplayObjects[idx];
			if(displayObject == null) continue;
			if(GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(displayObject.name)) {
				Element element = GlobalData.GetElement(displayObject.name);
				GlobalData.CurrentCopyDisplayObjects.Add(new Element {
					Name = element.Name,
					X = element.X,
					Y = element.Y,
					Width = element.Width,
					Height = element.Height,
					Visible = true
				});
			}
		}
	}

	public static Vector2 GetCopyDisplayObjectsLeftTop(List<Element> displayObjects) {
		if(displayObjects.Count == 0) return Vector2.zero;
		Vector2 result = new Vector2(displayObjects[0].Left, displayObjects[0].Top);
		int count = displayObjects.Count;
		for(int idx = 1; idx < count; ++ idx) {
			result.x = Math.Min(result.x, displayObjects[idx].X);
			result.y = Math.Min(result.y, displayObjects[idx].Y);
		}

		return result;
	}
}
