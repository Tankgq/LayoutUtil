using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
	public class DisplayObjectManager : MonoBehaviour, IDragHandler, IPointerDownHandler
	{
		private Vector2 _offset;

		public RectTransform SelfRect;

		public void OnDrag(PointerEventData eventData)
		{
			var mousePos = eventData.position;
			mousePos -= _offset;
			Vector3 pos;
			RectTransformUtility.ScreenPointToWorldPointInRectangle(SelfRect, mousePos, eventData.enterEventCamera, out pos);
			Vector3 offset = pos - SelfRect.position;
			// UpdateDisplayObjectPosition(SelfRect, transform.name, pos);
			SelfRect.position = pos;
			if (GlobalData.CurrentSelectDisplayObjectDic.Count == 1) return;
			foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic)
			{
				if (pair.Value == transform) continue;
				RectTransform rt = pair.Value.GetComponent<RectTransform>();
				// UpdateDisplayObjectPosition(rt, pair.Key, rt.position + offset);
				rt.position = pos;
			}
		}

		// private void UpdateDisplayObjectPosition(RectTransform rt, string name, Vector3 pos)
		// {
		// 	// rt.position = pos;
		// 	// DisplayObject displayObjectData = GlobalData.Modules[GlobalData.CurrentModule].Find(element => element.name.Equals(transform.name));
		// 	// if (displayObjectData == null) return;
		// 	// 延迟一帧才能拿到正确的 rt.anchoredPosition
		// 	// displayObjectData.x = DisplayObject.ConvertX(rt.anchoredPosition.x);
		// 	// displayObjectData.y = DisplayObject.ConvertY(rt.anchoredPosition.y);
		// }

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