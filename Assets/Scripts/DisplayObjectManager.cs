﻿using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

namespace Assets.Scripts
{
	public class DisplayObjectManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
	{
		private Vector2 _offset;
		public Vector2 Offset
		{
			set
			{
				_offset = value;
			}
		}
		private AlignInfo _alignInfo;
		private static GameObject HorizontalAlignLine = null;
		private static GameObject VerticalAlignLine = null;

		public RectTransform SelfRect;

		private void Start()
		{
			if (HorizontalAlignLine == null)
				HorizontalAlignLine = Instantiate<GameObject>(GlobalData.LinePrefab, GlobalData.DisplayObjectContainer.transform);
			if (VerticalAlignLine == null)
				VerticalAlignLine = Instantiate<GameObject>(GlobalData.LinePrefab, GlobalData.DisplayObjectContainer.transform);
			HorizontalAlignLine.SetActive(false);
			VerticalAlignLine.SetActive(false);
		}

		private static string _copying = null;
		private Vector2 _startPos = Vector2.zero;
		void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
		{
			if (KeyboardEventManager.GetAlt() && _copying == null)
			{
				string imageUrl = null;
				string key = $"{GlobalData.CurrentModule}_{transform.name}";
				if (GlobalData.DisplayObjectPathDic.ContainsKey(key))
					imageUrl = GlobalData.DisplayObjectPathDic[key];
				Vector2 pos = Element.InvConvertTo(SelfRect.anchoredPosition);
				Transform copyDisplayObject = GlobalData.ContainerManager.AddDisplayObject(imageUrl, pos, SelfRect.sizeDelta, transform.name + "_copy");
				if (copyDisplayObject == null) return;
				DisplayObjectManager dom = copyDisplayObject.GetComponent<DisplayObjectManager>();
				if (dom) dom.Offset = _offset;
				List<Transform> copies = new List<Transform>();
				copies.Add(copyDisplayObject);
				_copying = copyDisplayObject.name;
				foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic)
				{
					if (pair.Value == this.transform) continue;
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

				MessageBroker.Send(MessageBroker.UPDATE_SELECT_DISPLAY_OBJECT);
				ExecuteEvents.Execute<IEndDragHandler>(gameObject, eventData, ExecuteEvents.endDragHandler);
				eventData.pointerDrag = copies[0].gameObject;
				ExecuteEvents.Execute<IBeginDragHandler>(copyDisplayObject.gameObject, eventData, ExecuteEvents.beginDragHandler);
			}
			else
			{
				_startPos = SelfRect.anchoredPosition;
			}
		}

		public void OnDrag(PointerEventData eventData)
		{
			if (Input.GetMouseButton(2)) return;
			Vector2 pos = Utils.GetAnchoredPositionInContainer(Input.mousePosition) - _offset;
			Vector2 offset = pos - SelfRect.anchoredPosition;
			UpdateDisplayObjectPosition(SelfRect, transform.name, pos);
			_alignInfo = GlobalData.ContainerManager.GetAlignLine(transform);
			Rectangle horizontalAlignRect = _alignInfo.HorizontalAlignLine;
			if (horizontalAlignRect != null)
			{
				HorizontalAlignLine.SetActive(true);
				HorizontalAlignLine.transform.SetAsLastSibling();
				RectTransform rt = HorizontalAlignLine.GetComponent<RectTransform>();
				rt.anchoredPosition = Element.InvConvertTo(new Vector2(horizontalAlignRect.X - GlobalData.ALIGN_EXTENSION_VALUE, horizontalAlignRect.Y));
				rt.sizeDelta = new Vector2(horizontalAlignRect.Width + (GlobalData.ALIGN_EXTENSION_VALUE << 1), horizontalAlignRect.Height);
			}
			else HorizontalAlignLine.SetActive(false);
			Rectangle verticalAlignRect = _alignInfo.VerticalAlignLine;
			if (verticalAlignRect != null)
			{
				VerticalAlignLine.SetActive(true);
				VerticalAlignLine.transform.SetAsLastSibling();
				RectTransform rt = VerticalAlignLine.GetComponent<RectTransform>();
				rt.anchoredPosition = Element.InvConvertTo(new Vector2(verticalAlignRect.X, verticalAlignRect.Y - GlobalData.ALIGN_EXTENSION_VALUE));
				rt.sizeDelta = new Vector2(verticalAlignRect.Width, verticalAlignRect.Height + (GlobalData.ALIGN_EXTENSION_VALUE << 1));
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
			Vector2 pos = SelfRect.anchoredPosition;
			Vector2 size = SelfRect.sizeDelta;
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
			List<String> selectedDisplayObjects = new List<String>();
			selectedDisplayObjects.Add(transform.name);
			foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic)
			{
				if (pair.Value == transform) continue;
				selectedDisplayObjects.Add(pair.Key);
			}
			new Action<string, List<String>, Vector2, Vector2>((module, displayObjects, originPos, targetPos) =>
			{
				HistoryManager.Do(new Behavior(() => UpdateDisplayObjectsPosition(module, displayObjects, targetPos),
												() => UpdateDisplayObjectsPosition(module, displayObjects, originPos)));
			})(GlobalData.CurrentModule, selectedDisplayObjects, _startPos, pos);
		}

		private void UpdateDisplayObjectsPosition(String module, List<String> displayObjects, Vector2 targetPos)
		{
			if (displayObjects == null) return;
			int count = displayObjects.Count;
			if (count == 0) return;
			if (String.IsNullOrWhiteSpace(GlobalData.CurrentModule) || !GlobalData.CurrentModule.Equals(module))
				return;
			Transform baseDisplayObject = GlobalData.CurrentDisplayObjectDic[displayObjects[0]];
			RectTransform baseRect = baseDisplayObject.GetComponent<RectTransform>();
			if (baseRect == null) return;
			Vector2 offset = targetPos - baseRect.anchoredPosition;
			UpdateDisplayObjectPosition(baseRect, transform.name, targetPos);
			for (int idx = 1; idx < count; ++idx)
			{
				Transform displayObject = GlobalData.CurrentDisplayObjectDic[displayObjects[idx]];
				if (displayObject == null) continue;
				RectTransform rt = displayObject.GetComponent<RectTransform>();
				UpdateDisplayObjectPosition(rt, displayObjects[idx], rt.anchoredPosition + offset);
			}
		}

		private void UpdateDisplayObjectPosition(RectTransform rt, string name, Vector3 pos)
		{
			rt.anchoredPosition = pos;
			Element element = GlobalData.GetElement(name);
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
					MessageBroker.Send(MessageBroker.UPDATE_SELECT_DISPLAY_OBJECT);
				}
			}
			else
			{
				if (!KeyboardEventManager.GetShift())
					DeselectAllDisplayObject();
				GlobalData.CurrentSelectDisplayObjectDic.Add(transform.name, transform);
				MessageBroker.Send(MessageBroker.UPDATE_SELECT_DISPLAY_OBJECT);
			}
			var mousePos = eventData.position;
			Vector2 offset;
			var isRect = RectTransformUtility.ScreenPointToLocalPointInRectangle(SelfRect, mousePos, eventData.enterEventCamera, out offset);
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
}