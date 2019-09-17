using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DisplayObjectManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
{
	private Vector2 _offset;
	private AlignInfo _alignInfo;
	private static GameObject HorizontalAlignLine;
	private static GameObject VerticalAlignLine;

	public RectTransform selfRect;

	private void Start()
	{
		if (HorizontalAlignLine == null)
			HorizontalAlignLine = Instantiate(GlobalData.LinePrefab, GlobalData.DisplayObjectContainer.transform);
		if (VerticalAlignLine == null)
			VerticalAlignLine = Instantiate(GlobalData.LinePrefab, GlobalData.DisplayObjectContainer.transform);
		HorizontalAlignLine.SetActive(false);
		VerticalAlignLine.SetActive(false);
	}

	private static string _copying;
	private Vector2 _startPos = Vector2.zero;
	void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
	{
		if (KeyboardEventManager.GetAlt() && _copying == null)
		{
			string imageUrl = null;
			string key = $"{GlobalData.CurrentModule}_{transform.name}";
			if (GlobalData.DisplayObjectPathDic.ContainsKey(key))
				imageUrl = GlobalData.DisplayObjectPathDic[key];
			Vector2 pos = Element.InvConvertTo(selfRect.anchoredPosition);
			Transform copyDisplayObject = GlobalData.ContainerManager.AddDisplayObject(imageUrl, pos, selfRect.sizeDelta, transform.name + "_copy");
			if (copyDisplayObject == null) return;
			DisplayObjectManager dom = copyDisplayObject.GetComponent<DisplayObjectManager>();
			if (dom) dom._offset = _offset;
			List<Transform> copies = new List<Transform> {copyDisplayObject};
			_copying = copyDisplayObject.name;
			foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic)
			{
				if (pair.Value == transform) continue;
				Element element = GlobalData.GetElement(pair.Key);
				if (element == null) continue;
				key = $"{GlobalData.CurrentModule}_{pair.Key}";
				if (GlobalData.DisplayObjectPathDic.ContainsKey(pair.Key))
					imageUrl = GlobalData.DisplayObjectPathDic[key];
				copyDisplayObject = GlobalData.ContainerManager.AddDisplayObject(imageUrl,
																				 new Vector2(element.X, element.Y) + Element.InvConvertTo(GlobalData.OriginPoint),
																				 new Vector2(element.Width, element.Height),
																				 pair.Key + "_copy");
				copies.Add(copyDisplayObject);
			}

			GlobalData.CurrentSelectDisplayObjectDic.Clear();
			foreach (Transform displayObject in copies)
			{
				if (displayObject == null) continue;
				GlobalData.CurrentSelectDisplayObjectDic.Add(displayObject.name, displayObject);
			}

			MessageBroker.Send(MessageBroker.UpdateSelectDisplayObject);
			ExecuteEvents.Execute(gameObject, eventData, ExecuteEvents.endDragHandler);
			eventData.pointerDrag = copies[0].gameObject;
			ExecuteEvents.Execute(copyDisplayObject.gameObject, eventData, ExecuteEvents.beginDragHandler);
		}
		else
		{
			_startPos = selfRect.anchoredPosition;
		}
	}

	public void OnDrag(PointerEventData eventData)
	{
		if (Input.GetMouseButton(2)) return;
		Vector2 pos = Utils.GetAnchoredPositionInContainer(Input.mousePosition) - _offset;
		Vector2 offset = pos - selfRect.anchoredPosition;
		UpdateDisplayObjectPosition(selfRect, transform.name, pos);
		_alignInfo = GlobalData.ContainerManager.GetAlignLine(transform);
		Rectangle horizontalAlignRect = _alignInfo.HorizontalAlignLine;
		if (horizontalAlignRect != null)
		{
			HorizontalAlignLine.SetActive(true);
			HorizontalAlignLine.transform.SetAsLastSibling();
			RectTransform rt = HorizontalAlignLine.GetComponent<RectTransform>();
			rt.anchoredPosition = Element.InvConvertTo(new Vector2(horizontalAlignRect.X - GlobalData.AlignExtensionValue, horizontalAlignRect.Y));
			rt.sizeDelta = new Vector2(horizontalAlignRect.Width + (GlobalData.AlignExtensionValue << 1), horizontalAlignRect.Height);
		}
		else HorizontalAlignLine.SetActive(false);
		Rectangle verticalAlignRect = _alignInfo.VerticalAlignLine;
		if (verticalAlignRect != null)
		{
			VerticalAlignLine.SetActive(true);
			VerticalAlignLine.transform.SetAsLastSibling();
			RectTransform rt = VerticalAlignLine.GetComponent<RectTransform>();
			rt.anchoredPosition = Element.InvConvertTo(new Vector2(verticalAlignRect.X, verticalAlignRect.Y - GlobalData.AlignExtensionValue));
			rt.sizeDelta = new Vector2(verticalAlignRect.Width, verticalAlignRect.Height + (GlobalData.AlignExtensionValue << 1));
		}
		else VerticalAlignLine.SetActive(false);
		if (GlobalData.CurrentSelectDisplayObjectDic.Count == 1) return;
		foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic)
		{
			if (pair.Value == transform) continue;
			RectTransform rt = pair.Value.GetComponent<RectTransform>();
			UpdateDisplayObjectPosition(rt, pair.Key, rt.anchoredPosition + offset);
		}
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		if (transform.name.Equals(_copying))
			_copying = null;
		if (_alignInfo == null || KeyboardEventManager.GetControl())
		{
			HorizontalAlignLine.SetActive(false);
			VerticalAlignLine.SetActive(false);
			return;
		}
		Vector2 pos = selfRect.anchoredPosition;
		Vector2 size = selfRect.sizeDelta;
		if (VerticalAlignLine.activeSelf && _alignInfo.VerticalAlignLine != null)
		{
			pos.x = _alignInfo.VerticalAlignLine.Left;
			if (_alignInfo.VerticalAlignType == AlignInfo.ALIGN_RIGHT)
				pos.x -= size.x;
			pos.x = Element.InvConvertX(pos.x);
			VerticalAlignLine.SetActive(false);
		}
		if (HorizontalAlignLine.activeSelf && _alignInfo.HorizontalAlignLine != null)
		{
			pos.y = _alignInfo.HorizontalAlignLine.Top;
			if (_alignInfo.HorizontalAlignType == AlignInfo.ALIGN_BOTTOM)
				pos.y -= size.y;
			pos.y = Element.InvConvertY(pos.y);
			HorizontalAlignLine.SetActive(false);
		}

		List<string> selectedDisplayObjects = new List<String> {transform.name};
		foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic)
		{
			if (pair.Value == transform) continue;
			selectedDisplayObjects.Add(pair.Key);
		}
		new Action<string, List<string>, Vector2, Vector2>((module, displayObjects, originPos, targetPos) =>
		{
			HistoryManager.Do(new Behavior(() => UpdateDisplayObjectsPosition(module, displayObjects, targetPos, true),
										   () => UpdateDisplayObjectsPosition(module, displayObjects, originPos, false)));
		})(GlobalData.CurrentModule, selectedDisplayObjects, _startPos, pos);
	}

	private void UpdateDisplayObjectsPosition(String module, List<String> displayObjects, Vector2 targetPos, bool isModify)
	{
		if (string.IsNullOrWhiteSpace(GlobalData.CurrentModule) || !GlobalData.CurrentModule.Equals(module))
			return;
		if (displayObjects == null || displayObjects.Count == 0) return;
		Transform baseDisplayObject = GlobalData.CurrentDisplayObjectDic[displayObjects[0]];
		if(baseDisplayObject == null) return;
		RectTransform baseRect = baseDisplayObject.GetComponent<RectTransform>();
		if (baseRect == null) return;
		Vector2 offset = targetPos - baseRect.anchoredPosition;
		UpdateDisplayObjectPosition(baseRect, transform.name, targetPos);
		int count = displayObjects.Count;
		for (int idx = 1; idx < count; ++idx)
		{
			Transform displayObject = GlobalData.CurrentDisplayObjectDic[displayObjects[idx]];
			if (displayObject == null) continue;
			RectTransform rt = displayObject.GetComponent<RectTransform>();
			UpdateDisplayObjectPosition(rt, displayObjects[idx], rt.anchoredPosition + offset);
		}
		GlobalData.ModifyCount += isModify ? 1 : -1;
		MessageBroker.Send(MessageBroker.UpdateDisplayOjectPos);
	}

	private void UpdateDisplayObjectPosition(RectTransform rt, string elementName, Vector3 pos)
	{
		rt.anchoredPosition = pos;
		Element element = GlobalData.GetElement(elementName);
		if (element == null) return;
		element.X = Element.ConvertX(pos.x);
		element.Y = Element.ConvertY(pos.y);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (Input.GetMouseButton(2)) return;
		bool isSelect = GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(transform.name);
		if (isSelect)
		{
			if (KeyboardEventManager.GetControl())
			{
				GlobalData.CurrentSelectDisplayObjectDic.Remove(transform.name);
				MessageBroker.Send(MessageBroker.UpdateSelectDisplayObject);
			}
		}
		else
		{
			if (!KeyboardEventManager.GetShift())
				DeselectAllDisplayObject();
			Transform self = transform;
			GlobalData.CurrentSelectDisplayObjectDic.Add(self.name, self);
			MessageBroker.Send(MessageBroker.UpdateSelectDisplayObject);
		}
		var mousePos = eventData.position;
		Vector2 offset;
		var isRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(selfRect, mousePos, eventData.enterEventCamera, out offset);
		if (isRect) _offset = offset;
	}

	public static bool DeSelectDisplayObject(Transform displayObject)
	{
		if (!displayObject) return false;
		if (!GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(displayObject.name)) return false;
		GlobalData.CurrentSelectDisplayObjectDic.Remove(displayObject.name);
		return true;
	}

	public static void DeselectAllDisplayObject()
	{
		GlobalData.CurrentSelectDisplayObjectDic.Clear();
	}
}