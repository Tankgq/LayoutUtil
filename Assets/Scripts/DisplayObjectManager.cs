using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
	public class DisplayObjectManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
	{
		private Vector2 _offset;
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
		void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
		{
			print($"startDrag: {transform.name}");
			if (KeyboardEventManager.GetAlt() && _copying == null)
			{
				print("copy");
				string imageUrl = null;
				string key = $"{GlobalData.CurrentModule}_{transform.name}";
				if (GlobalData.DisplayObjectPathDic.ContainsKey(key))
					imageUrl = GlobalData.DisplayObjectPathDic[key];
				Vector2 pos = Element.InvConvertTo(SelfRect.anchoredPosition);
				Transform newDisplayObject = GlobalData.ContainerManager.AddDisplayObject(imageUrl, pos, SelfRect.sizeDelta, transform.name);
				if (newDisplayObject != null)
				{
					_copying = newDisplayObject.name;
					GlobalData.CurrentSelectDisplayObjectDic.Clear();
					GlobalData.CurrentSelectDisplayObjectDic.Add(newDisplayObject.name, newDisplayObject);
					MessageBroker.Send(MessageBroker.UPDATE_SELECT_DISPLAY_OBJECT);
					ExecuteEvents.Execute<IEndDragHandler>(gameObject, eventData, ExecuteEvents.endDragHandler);
					eventData.pointerDrag = newDisplayObject.gameObject;
					ExecuteEvents.Execute<IBeginDragHandler>(newDisplayObject.gameObject, eventData, ExecuteEvents.beginDragHandler);
				}
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
			Vector2 offset = pos - SelfRect.anchoredPosition;
			UpdateDisplayObjectPosition(SelfRect, transform.name, pos);
			if (GlobalData.CurrentSelectDisplayObjectDic.Count == 1) return;
			foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic)
			{
				if (pair.Value == transform) continue;
				RectTransform rt = pair.Value.GetComponent<RectTransform>();
				UpdateDisplayObjectPosition(rt, pair.Key, rt.anchoredPosition + offset);
			}
		}

		private void UpdateDisplayObjectPosition(RectTransform rt, string name, Vector3 pos)
		{
			rt.anchoredPosition = pos;
			Element displayObjectData = GlobalData.GetElement(name);
			if (displayObjectData == null) return;
			displayObjectData.X = Element.ConvertX(pos.x);
			displayObjectData.Y = Element.ConvertY(pos.y);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (Input.GetMouseButton(2)) return;
			bool isSelect = GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(transform.name);
			print($"point: {transform.name}");
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