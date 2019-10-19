using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class DisplayObjectManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler {
	private bool _isDrag;
	private Vector2 _offset;
	private AlignInfo _alignInfo;
	private static GameObject _horizontalAlignLine;
	private static GameObject _verticalAlignLine;

	public RectTransform selfRect;

	private void Start() {
		if(_horizontalAlignLine == null) _horizontalAlignLine = Instantiate(GlobalData.LinePrefab, GlobalData.DisplayObjectContainer.transform);
		if(_verticalAlignLine == null) _verticalAlignLine = Instantiate(GlobalData.LinePrefab, GlobalData.DisplayObjectContainer.transform);
		_horizontalAlignLine.SetActive(false);
		_verticalAlignLine.SetActive(false);
	}

	private static string _copying;
	private Vector2 _startPos = Vector2.zero;

	void IBeginDragHandler.OnBeginDrag(PointerEventData eventData) {
		_isDrag = true;
		if(KeyboardEventManager.GetAlt() && _copying == null) {
			string imageUrl = DisplayObjectUtil.GetImageUrl(GlobalData.CurrentModule, transform.name);
			Vector2 pos = Element.InvConvertTo(selfRect.anchoredPosition);
			Transform copyDisplayObject = DisplayObjectUtil.AddDisplayObject(imageUrl, pos, selfRect.sizeDelta, transform.name + "_copy");
			if(copyDisplayObject == null) return;
			DisplayObjectManager dom = copyDisplayObject.GetComponent<DisplayObjectManager>();
			if(dom) dom._offset = _offset;
			List<Transform> copies = new List<Transform> {copyDisplayObject};
			_copying = copyDisplayObject.name;
			foreach(var pair in GlobalData.CurrentSelectDisplayObjectDic) {
				if(pair.Value == transform) continue;
				Element element = GlobalData.GetElement(pair.Key);
				if(element == null) continue;
				string key = $"{GlobalData.CurrentModule}_{pair.Key}";
				if(GlobalData.DisplayObjectPathDic.ContainsKey(pair.Key)) imageUrl = GlobalData.DisplayObjectPathDic[key];
				copyDisplayObject = DisplayObjectUtil.AddDisplayObject(imageUrl,
																	   new Vector2(element.X, element.Y) + Element.InvConvertTo(GlobalData.OriginPoint),
																	   new Vector2(element.Width, element.Height),
																	   pair.Key + "_copy");
				copies.Add(copyDisplayObject);
			}

			List<string> addElements = copies.Select(element => element.name).ToList();
			List<string> removeElements = null;
			if(GlobalData.CurrentSelectDisplayObjectDic.Count > 0) {
				removeElements = GlobalData.CurrentSelectDisplayObjectDic.Select(pair => pair.Key).ToList();
				GlobalData.CurrentSelectDisplayObjectDic.Clear();
			}

			foreach(Transform displayObject in copies) {
				GlobalData.CurrentSelectDisplayObjectDic.Add(displayObject.name, displayObject);
			}

			HistoryManager.Do(BehaviorFactory.GetUpdateSelectDisplayObjectBehavior(GlobalData.CurrentModule, addElements, removeElements, true));
			HistoryManager.Do(BehaviorFactory.GetCopyDisplayObjectsBehavior(GlobalData.CurrentModule, addElements, true), true);
			ExecuteEvents.Execute(gameObject, eventData, ExecuteEvents.endDragHandler);
			eventData.pointerDrag = copies[0].gameObject;
			ExecuteEvents.Execute(copyDisplayObject.gameObject, eventData, ExecuteEvents.beginDragHandler);
		} else {
			_startPos = selfRect.anchoredPosition;
		}
	}

	public void OnDrag(PointerEventData eventData) {
		if(Input.GetMouseButton(2)) return;
		Vector2 pos = Utils.GetAnchoredPositionInContainer(Input.mousePosition) - _offset;
		Vector2 offset = pos - selfRect.anchoredPosition;
		DisplayObjectUtil.UpdateDisplayObjectPosition(selfRect, transform.name, pos);
		_alignInfo = DisplayObjectUtil.GetAlignLine(transform);
		Rectangle horizontalAlignRect = _alignInfo.HorizontalAlignLine;
		if(horizontalAlignRect != null) {
			_horizontalAlignLine.SetActive(true);
			_horizontalAlignLine.transform.SetAsLastSibling();
			RectTransform rt = _horizontalAlignLine.GetComponent<RectTransform>();
			rt.anchoredPosition = Element.InvConvertTo(new Vector2(horizontalAlignRect.X - GlobalData.AlignExtensionValue, horizontalAlignRect.Y));
			rt.sizeDelta = new Vector2(horizontalAlignRect.Width + (GlobalData.AlignExtensionValue << 1), horizontalAlignRect.Height);
		} else
			_horizontalAlignLine.SetActive(false);

		Rectangle verticalAlignRect = _alignInfo.VerticalAlignLine;
		if(verticalAlignRect != null) {
			_verticalAlignLine.SetActive(true);
			_verticalAlignLine.transform.SetAsLastSibling();
			RectTransform rt = _verticalAlignLine.GetComponent<RectTransform>();
			rt.anchoredPosition = Element.InvConvertTo(new Vector2(verticalAlignRect.X, verticalAlignRect.Y - GlobalData.AlignExtensionValue));
			rt.sizeDelta = new Vector2(verticalAlignRect.Width, verticalAlignRect.Height + (GlobalData.AlignExtensionValue << 1));
		} else
			_verticalAlignLine.SetActive(false);

		if(GlobalData.CurrentSelectDisplayObjectDic.Count == 1) return;
		foreach(var pair in GlobalData.CurrentSelectDisplayObjectDic) {
			if(pair.Value == transform) continue;
			RectTransform rt = pair.Value.GetComponent<RectTransform>();
			DisplayObjectUtil.UpdateDisplayObjectPosition(rt, pair.Key, rt.anchoredPosition + offset);
		}
	}

	public void OnEndDrag(PointerEventData eventData) {
		_isDrag = false;
		if(transform.name.Equals(_copying)) _copying = null;
		if(_alignInfo == null || KeyboardEventManager.GetControl()) {
			_horizontalAlignLine.SetActive(false);
			_verticalAlignLine.SetActive(false);
			return;
		}

		Vector2 pos = selfRect.anchoredPosition;
		Vector2 size = selfRect.sizeDelta;
		if(_verticalAlignLine.activeSelf && _alignInfo.VerticalAlignLine != null) {
			pos.x = _alignInfo.VerticalAlignLine.Left;
			if(_alignInfo.VerticalAlignType == AlignType.Right) pos.x -= size.x;
			pos.x = Element.InvConvertX(pos.x);
			_verticalAlignLine.SetActive(false);
		}

		if(_horizontalAlignLine.activeSelf && _alignInfo.HorizontalAlignLine != null) {
			pos.y = _alignInfo.HorizontalAlignLine.Top;
			if(_alignInfo.HorizontalAlignType == AlignType.Bottom) pos.y -= size.y;
			pos.y = Element.InvConvertY(pos.y);
			_horizontalAlignLine.SetActive(false);
		}

		List<string> selectedDisplayObjects = new List<string> {transform.name};
		selectedDisplayObjects.AddRange(from pair in GlobalData.CurrentSelectDisplayObjectDic where pair.Value != transform select pair.Key);

		HistoryManager.Do(BehaviorFactory.GetUpdateDisplayObjectsPosBehavior(GlobalData.CurrentModule, selectedDisplayObjects, _startPos, pos));
	}

	public void OnPointerDown(PointerEventData eventData) {
		if(Input.GetMouseButton(2)) return;
		Transform self = transform;
		bool isSelect = GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(self.name);
		if(isSelect) {
			if(KeyboardEventManager.GetControl()) {
				HistoryManager.Do(BehaviorFactory.GetUpdateSelectDisplayObjectBehavior(GlobalData.CurrentModule, null, new List<string> {self.name}));
			}
		} else {
			List<string> removeElements = null;
			if(! KeyboardEventManager.GetShift()) {
				if(GlobalData.CurrentSelectDisplayObjectDic.Count > 0) removeElements = GlobalData.CurrentSelectDisplayObjectDic.Select(pair => pair.Key).ToList();
			}

			HistoryManager.Do(BehaviorFactory.GetUpdateSelectDisplayObjectBehavior(GlobalData.CurrentModule, new List<string> {self.name}, removeElements));
		}

		Vector2 mousePos = eventData.position;
		Vector2 offset;
		bool isRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(selfRect, mousePos, eventData.enterEventCamera, out offset);
		if(isRect) _offset = offset;
	}

	public static void DeSelectDisplayObject(Transform displayObject) {
		if(! displayObject) return;
		if(! GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(displayObject.name)) return;
		GlobalData.CurrentSelectDisplayObjectDic.Remove(displayObject.name);
	}

	public void OnPointerUp(PointerEventData eventData) {
		if(_isDrag || KeyboardEventManager.GetShift() || KeyboardEventManager.GetControl() || GlobalData.CurrentSelectDisplayObjectDic.Count <= 1) return;
		List<string> removeElements = GlobalData.CurrentSelectDisplayObjectDic
												.Where(pair => ! pair.Key.Equals(transform.name))
												.Select(pair => pair.Key)
												.ToList();
		HistoryManager.Do(BehaviorFactory.GetUpdateSelectDisplayObjectBehavior(GlobalData.CurrentModule, null, removeElements));
	}
}
