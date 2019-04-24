using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
	public class DisplayObjectManager : MonoBehaviour, IDragHandler, IPointerDownHandler
	{
		private Vector2 _offset;

		public RectTransform SelfRect;
		//    public Toggle SelfToggle;

		public void OnDrag(PointerEventData eventData)
		{
			var mousePos = eventData.position;
			mousePos -= _offset;
			Vector3 pos;
			RectTransformUtility.ScreenPointToWorldPointInRectangle(SelfRect, mousePos, eventData.enterEventCamera, out pos);
			SelfRect.position = pos;
			DisplayObject displayObjectData = GlobalData.Modules[GlobalData.CurrentModule].Find(element => element.name.Equals(transform.name));
			if (displayObjectData == null) return;
			displayObjectData.x = DisplayObject.ConvertX(pos.x);
			displayObjectData.y = DisplayObject.ConvertY(pos.y);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			string displayObjectKey = $"{GlobalData.CurrentModule}_{transform.name}";
			bool isSelect = GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(displayObjectKey);
			if (isSelect)
			{
				if (KeyboardEventManager.IsControlDown())
					GlobalData.CurrentSelectDisplayObjectDic.Remove(displayObjectKey);
			}
			else
			{
				if (!KeyboardEventManager.IsShiftDown())
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
			string displayObjectKey = $"{GlobalData.CurrentModule}_{displayObject.name}";
			if (!GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(displayObjectKey)) return false;
			GlobalData.CurrentSelectDisplayObjectDic.Remove(displayObjectKey);
			return true;
		}

		public static void DeselectAllDisplayObject()
		{
			GlobalData.CurrentSelectDisplayObjectDic.Clear();
		}
	}
}