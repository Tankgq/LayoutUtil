using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorkSpaceManager : MonoBehaviour, IPointerDownHandler {
	public void OnPointerDown(PointerEventData eventData) {
		if(Input.GetMouseButtonDown(0))
			OnMouseLeftButtonDown();
		else if(Input.GetMouseButton(1)) OnMouseRightButtonDown();

		// 	if (string.IsNullOrWhiteSpace(GlobalData.CurrentModule)) return;
		// 	Vector2 pos = Element.ConvertTo(Utils.GetAnchoredPositionInContainer(Input.mousePosition));
		// 	int length = GlobalData.CurrentDisplayObjects.Count;
		// 	for (int idx = 0; idx < length; ++idx)
		// 	{
		// 		Transform displayObject = GlobalData.CurrentDisplayObjects[idx];
		// 		if (displayObject == null) continue;
		// 		if (CheckPointerExpand(displayObject, pos))
		// 		{
		// 			eventData.pointerEnter = displayObject.gameObject;
		// 			eventData.pointerPress = displayObject.gameObject;
		// 			eventData.pointerDrag = displayObject.gameObject;
		// 			ExecuteEvents.Execute<IPointerDownHandler>(displayObject.gameObject, eventData, ExecuteEvents.pointerDownHandler);
		// 			ExecuteEvents.Execute<IBeginDragHandler>(displayObject.gameObject, eventData, ExecuteEvents.beginDragHandler);
		// 			return;
		// 		}
		// 	}
	}

	// private bool CheckPointerExpand(Transform displayObject, Vector2 pos)
	// {
	// 	if (string.IsNullOrWhiteSpace(GlobalData.CurrentModule)) return false;
	// 	// Vector2 pos = Element.ConvertTo(Utils.GetAnchoredPositionInContainer(Input.mousePosition));
	// 	Element element = GlobalData.GetElement(displayObject.name);
	// 	if (element == null) return false;
	// 	return element.Contain(pos, GlobalData.ExpandValue);
	// }

	private static void OnMouseLeftButtonDown() {
		if(KeyboardEventManager.GetControl() || KeyboardEventManager.GetShift()) return;
		List<string> removeElements = null;
		if(GlobalData.CurrentSelectDisplayObjectDic.Count > 0) removeElements = GlobalData.CurrentSelectDisplayObjectDic.Select(pair => pair.Key).ToList();
		if(removeElements != null) HistoryManager.Do(BehaviorFactory.GetUpdateSelectDisplayObjectBehavior(GlobalData.CurrentModule, null, removeElements));
	}

	private static void OnMouseRightButtonDown() {
		Debug.Log("OnMouseRightDown");
	}
}
