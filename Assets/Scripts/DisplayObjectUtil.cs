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
			AddDisplayObjectBehavior(GlobalData.CurrentModule, elements[idx], null, false);
		}
	}

	public static void AddDisplayObjectBehavior(string moduleName, Element element, string imageUrl = null, bool addToModule = true) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! moduleName.Equals(GlobalData.CurrentModule)) return;
		if(element == null) return;
		if(addToModule) GlobalData.ModuleDic[moduleName].Add(element);
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
		foreach(string elementName in elements) RemoveDisplayObjectBehavior(moduleName, elementName);
	}

	public static void RemoveDisplayObjectsBehavior(string moduleName, List<Element> elements) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! moduleName.Equals(GlobalData.CurrentModule)) return;
		if(elements == null || elements.Count == 0) return;
		foreach(Element element in elements) RemoveDisplayObjectBehavior(moduleName, element.Name);
	}

	public static void AddDisplayObjectsBehavior(string moduleName, List<Element> elements) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! moduleName.Equals(GlobalData.CurrentModule)) return;
		if(elements == null || elements.Count == 0) return;
		foreach(Element element in elements) AddDisplayObjectBehavior(moduleName, element);
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

	public static string GetImageUrl(string moduleName, string elementName) {
		string imageUrl;
		string key = $"{moduleName}_{elementName}";
		GlobalData.DisplayObjectPathDic.TryGetValue(key, out imageUrl);
		return imageUrl;
	}

	public static string GetCanUseElementName(string sourceElementName = null, string imageUrl = null) {
		++ GlobalData.UniqueId;
		string elementName = string.IsNullOrWhiteSpace(sourceElementName)
								? string.IsNullOrWhiteSpace(imageUrl) ? GlobalData.DefaultName + GlobalData.UniqueId : Utils.GetFileNameInPath(imageUrl)
								: sourceElementName;
		if(! GlobalData.CurrentDisplayObjectDic.ContainsKey(elementName)) return elementName;
		++ GlobalData.UniqueId;
		elementName += GlobalData.UniqueId;
		return elementName;
	}

	public static Transform AddDisplayObject(string imageUrl, Vector2 pos, Vector2 size, string sourceElementName = null) {
		if(string.IsNullOrEmpty(GlobalData.CurrentModule)) {
			if(GlobalData.ModuleDic.Count == 0) {
				DialogManager.ShowInfo("请先创建一个 module", KeyCode.Return, 320);
				return null;
			}

			DialogManager.ShowInfo("请先打开一个 module", KeyCode.Return, 320);
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

		string elementName = GetCanUseElementName(sourceElementName, imageUrl);
		pos = Element.ConvertTo(pos);

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

		HistoryManager.Do(BehaviorFactory.GetRemoveSelectedDisplayObjectBehavior(GlobalData.CurrentModule));
	}

	public static void MoveDisplayObjectsUpBehavior(string moduleName, List<string> elementNames) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! moduleName.Equals(GlobalData.CurrentModule)) return;
		if(elementNames == null || elementNames.Count == 0) return;
		List<Transform> displayObjects = GlobalData.CurrentDisplayObjects;
		List<int> elementIdxList = elementNames.Select(elementName => displayObjects.FindIndex(element => elementName.Equals(element.name)))
											   .Where(idx => idx != -1)
											   .ToList();
		if(elementIdxList.Count == 0) return;
		List<Element> elements = GlobalData.ModuleDic[moduleName];
		int count = elementIdxList.Count;
		elementIdxList.Sort();
		for(int idx = 0; idx < count; ++ idx) {
			int elementIdx = elementIdxList[idx];
			Transform tmp = displayObjects[elementIdx];
			displayObjects[elementIdx] = displayObjects[elementIdx - 1];
			displayObjects[elementIdx - 1] = tmp;
			Element tmpElement = elements[elementIdx];
			elements[elementIdx] = elements[elementIdx - 1];
			elements[elementIdx - 1] = tmpElement;
			int siblingIndex = displayObjects[elementIdx].GetSiblingIndex();
			tmp.SetSiblingIndex(siblingIndex - 1);
		}

		MessageBroker.SendUpdateHierarchy();
	}

	public static void MoveDisplayObjectsDownBehavior(string moduleName, List<string> elementNames) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! moduleName.Equals(GlobalData.CurrentModule)) return;
		if(elementNames == null || elementNames.Count == 0) return;
		List<Transform> displayObjects = GlobalData.CurrentDisplayObjects;
		List<int> elementIdxList = elementNames.Select(elementName => displayObjects.FindIndex(element => elementName.Equals(element.name)))
											   .Where(idx => idx != -1)
											   .ToList();
		if(elementIdxList.Count == 0) return;
		List<Element> elements = GlobalData.ModuleDic[moduleName];
		int count = elementIdxList.Count;
		elementIdxList.Sort((lhs, rhs) => lhs < rhs ? 1 : (lhs == rhs ? 0 : -1));
		for(int idx = 0; idx < count; ++ idx) {
			int elementIdx = elementIdxList[idx];
			Transform tmp = displayObjects[elementIdx];
			displayObjects[elementIdx] = displayObjects[elementIdx + 1];
			displayObjects[elementIdx + 1] = tmp;
			Element tmpElement = elements[elementIdx];
			elements[elementIdx] = elements[elementIdx + 1];
			elements[elementIdx + 1] = tmpElement;
			int siblingIndex = displayObjects[elementIdx].GetSiblingIndex();
			tmp.SetSiblingIndex(siblingIndex + 1);
		}

		MessageBroker.SendUpdateHierarchy();
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
			if(! GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(displayObject.name)) continue;
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

	public static Vector2 GetCopyDisplayObjectsLeftTop(List<Element> elements) {
		if(elements.Count == 0) return Vector2.zero;
		Vector2 result = new Vector2(elements[0].Left, elements[0].Top);
		int count = elements.Count;
		for(int idx = 1; idx < count; ++ idx) {
			result.x = Math.Min(result.x, elements[idx].X);
			result.y = Math.Min(result.y, elements[idx].Y);
		}

		return result;
	}

	public static void UpdateDisplayObjectsPosition(string moduleName, IReadOnlyList<string> elementNames, Vector2 targetPos) {
		if(string.IsNullOrWhiteSpace(GlobalData.CurrentModule) || ! GlobalData.CurrentModule.Equals(moduleName)) return;
		if(elementNames == null || elementNames.Count == 0) return;
		Transform baseDisplayObject = GlobalData.CurrentDisplayObjectDic[elementNames[0]];
		if(baseDisplayObject == null) return;
		RectTransform baseRect = baseDisplayObject.GetComponent<RectTransform>();
		if(baseRect == null) return;
		Vector2 offset = targetPos - baseRect.anchoredPosition;
		UpdateDisplayObjectPosition(baseRect, elementNames[0], targetPos);
		int count = elementNames.Count;
		for(int idx = 1; idx < count; ++ idx) {
			Transform displayObject = GlobalData.CurrentDisplayObjectDic[elementNames[idx]];
			if(displayObject == null) continue;
			RectTransform rt = displayObject.GetComponent<RectTransform>();
			UpdateDisplayObjectPosition(rt, elementNames[idx], rt.anchoredPosition + offset);
		}

		MessageBroker.SendUpdateInspectorInfo();
	}

	public static void UpdateDisplayObjectPosition(RectTransform rt, string elementName, Vector3 pos) {
		rt.anchoredPosition = pos;
		Element element = GlobalData.GetElement(elementName);
		if(element == null) return;
		element.X = Element.ConvertX(pos.x);
		element.Y = Element.ConvertY(pos.y);
	}

	public static void ChangeNameBehavior(string moduleName, string originName, string newName) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! GlobalData.CurrentModule.Equals(moduleName)) return;
		Transform displayObject = GlobalData.CurrentDisplayObjectDic[originName];
		if(displayObject == null) return;
		displayObject.name = newName;
		GlobalData.CurrentDisplayObjectDic.Add(newName, displayObject);
		GlobalData.CurrentDisplayObjectDic.Remove(originName);
		// HierarchyManager.UpdateDisplayObjectName(originName, newName);
		Element element = GlobalData.GetElement(originName);
		if(element != null) element.Name = newName;
		if(! GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(originName)) return;
		UpdateSelectDisplayObjectDicBehavior(moduleName, new List<string> {newName}, new List<string> {originName});
	}

	public static void ChangeXBehavior(string moduleName, List<string> elementNames, float x, bool isAdd = false) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! GlobalData.CurrentModule.Equals(moduleName)) return;
		if(elementNames == null || elementNames.Count == 0) return;
		int length = elementNames.Count;
		for(int idx = 0; idx < length; ++ idx) ChangeXBehavior(elementNames[idx], x, isAdd);
		MessageBroker.SendUpdateInspectorInfo();
	}

	private static void ChangeXBehavior(string elementName, float x, bool isAdd = false) {
		Transform displayObject = GlobalData.CurrentDisplayObjectDic[elementName];
		if(! displayObject) return;
		RectTransform rect = displayObject.GetComponent<RectTransform>();
		Vector2 pos = rect.anchoredPosition;
		if(isAdd)
			pos.x += x;
		else
			pos.x = Element.InvConvertX(x);
		rect.anchoredPosition = pos;
		Element element = GlobalData.GetElement(elementName);
		if(element == null) return;
		if(isAdd)
			element.X += x;
		else
			element.X = x;
	}

	public static void ChangeYBehavior(string moduleName, List<string> elementNames, float y, bool isAdd = false) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! GlobalData.CurrentModule.Equals(moduleName)) return;
		if(elementNames == null || elementNames.Count == 0) return;
		int length = elementNames.Count;
		for(int idx = 0; idx < length; ++ idx) ChangeYBehavior(elementNames[idx], y, isAdd);
		MessageBroker.SendUpdateInspectorInfo();
	}

	private static void ChangeYBehavior(string elementName, float y, bool isAdd = false) {
		Transform displayObject = GlobalData.CurrentDisplayObjectDic[elementName];
		if(! displayObject) return;
		RectTransform rect = displayObject.GetComponent<RectTransform>();
		Vector2 pos = rect.anchoredPosition;
		if(isAdd)
			pos.y += y;
		else
			pos.y = Element.InvConvertY(y);
		rect.anchoredPosition = pos;
		Element element = GlobalData.GetElement(elementName);
		if(element == null) return;
		if(isAdd)
			element.Y += y;
		else
			element.Y = y;
	}

	public static void ChangeWidthBehavior(string moduleName, List<string> elementNames, float width, bool isAdd = false) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! GlobalData.CurrentModule.Equals(moduleName)) return;
		if(elementNames == null || elementNames.Count == 0) return;
		int length = elementNames.Count;
		for(int idx = 0; idx < length; ++ idx) ChangeWidthBehavior(elementNames[idx], width, isAdd);
		MessageBroker.SendUpdateInspectorInfo();
	}

	private static void ChangeWidthBehavior(string elementName, float width, bool isAdd = false) {
		Transform displayObject = GlobalData.CurrentDisplayObjectDic[elementName];
		if(! displayObject) return;
		RectTransform rect = displayObject.GetComponent<RectTransform>();
		Vector2 size = rect.sizeDelta;
		size.x = isAdd ? Math.Max(size.x + width, 0) : Math.Max(width, 0);
		rect.sizeDelta = size;
		Element element = GlobalData.GetElement(elementName);
		if(element == null) return;
		if(isAdd)
			element.Width += width;
		else
			element.Width = width;
	}

	public static void ChangeHeightBehavior(string moduleName, List<string> elementNames, float height, bool isAdd = false) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! GlobalData.CurrentModule.Equals(moduleName)) return;
		if(elementNames == null || elementNames.Count == 0) return;
		int length = elementNames.Count;
		for(int idx = 0; idx < length; ++ idx) ChangeHeightBehavior(elementNames[idx], height, isAdd);
		MessageBroker.SendUpdateInspectorInfo();
	}

	private static void ChangeHeightBehavior(string elementName, float height, bool isAdd = false) {
		Transform displayObject = GlobalData.CurrentDisplayObjectDic[elementName];
		if(! displayObject) return;
		RectTransform rect = displayObject.GetComponent<RectTransform>();
		Vector2 size = rect.sizeDelta;
		size.y = isAdd ? Math.Max(size.y + height, 0) : Math.Max(height, 0);
		rect.sizeDelta = size;
		Element element = GlobalData.GetElement(elementName);
		if(element == null) return;
		if(isAdd)
			element.Height += height;
		else
			element.Height = height;
	}

	public static void UpdateSelectDisplayObjectDicBehavior(string moduleName, List<string> addElements = null, List<string> removeElements = null) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! GlobalData.CurrentModule.Equals(moduleName)) return;
		if(addElements?.Count > 0) {
			foreach(string elementName in addElements) {
				GlobalData.CurrentSelectDisplayObjectDic[elementName] = GlobalData.CurrentDisplayObjectDic[elementName];
			}
		}

		if(removeElements?.Count > 0) {
			foreach(string elementName in removeElements) {
				GlobalData.CurrentSelectDisplayObjectDic.Remove(elementName);
			}
		}

		MessageBroker.SendUpdateSelectDisplayObjectDic(addElements, removeElements);
	}

	public static void CopySelectDisplayObjectsBehavior(string moduleName, List<string> elementNames) {
		if(string.IsNullOrWhiteSpace(moduleName) || ! GlobalData.CurrentModule.Equals(moduleName)) return;
		if(elementNames == null || elementNames.Count == 0) return;
		List<Element> elements = elementNames.Select(GlobalData.GetElement).ToList();
		AddDisplayObjectsBehavior(moduleName, elements);
		List<string> removeElements = null;
		if(GlobalData.CurrentSelectDisplayObjectDic.Count > 0) removeElements = GlobalData.CurrentSelectDisplayObjectDic.Select(pair => pair.Key).ToList();
		HistoryManager.Do(BehaviorFactory.GetUpdateSelectDisplayObjectBehavior(moduleName, elementNames, removeElements));
	}
}
