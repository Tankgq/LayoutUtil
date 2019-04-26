using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
	public class DisplayObjectManager : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerDownHandler
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

		public void OnDrag(PointerEventData eventData)
		{
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
				rt.anchoredPosition = DisplayObject.InvConvertTo(new Vector2(horizontalAlignRect.X - GlobalData.ALIGN_EXTENSION_VALUE, horizontalAlignRect.Y));
				rt.sizeDelta = new Vector2(horizontalAlignRect.Width + (GlobalData.ALIGN_EXTENSION_VALUE << 1), horizontalAlignRect.Height);
			}
			else HorizontalAlignLine.SetActive(false);
			Rectangle verticalAlignRect = _alignInfo.VerticalAlignLine;
			if (verticalAlignRect != null)
			{
				VerticalAlignLine.SetActive(true);
				VerticalAlignLine.transform.SetAsLastSibling();
				RectTransform rt = VerticalAlignLine.GetComponent<RectTransform>();
				rt.anchoredPosition = DisplayObject.InvConvertTo(new Vector2(verticalAlignRect.X, verticalAlignRect.Y - GlobalData.ALIGN_EXTENSION_VALUE));
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
				pos.x = DisplayObject.InvConvertX(pos.x);
				VerticalAlignLine.SetActive(false);
			}
			if (HorizontalAlignLine.activeSelf && _alignInfo.HorizontalAlignLine != null)
			{
				pos.y = _alignInfo.HorizontalAlignLine.Top;
				if (_alignInfo.HorizontalAlignType == AlignInfo.ALIGN_BOTTOM)
					pos.y -= size.y;
				pos.y = DisplayObject.InvConvertY(pos.y);
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
			DisplayObject displayObjectData = GlobalData.GetDisplayObjectData(name);
			if (displayObjectData == null) return;
			displayObjectData.X = DisplayObject.ConvertX(pos.x);
			displayObjectData.Y = DisplayObject.ConvertY(pos.y);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			bool isSelect = GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(transform.name);
			if (isSelect)
			{
				if (KeyboardEventManager.GetControl())
					GlobalData.CurrentSelectDisplayObjectDic.Remove(transform.name);
			}
			else
			{
				if (!KeyboardEventManager.GetShift())
					DeselectAllDisplayObject();
				GlobalData.AddCurrentSelectObject(GlobalData.CurrentModule, this.transform);
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