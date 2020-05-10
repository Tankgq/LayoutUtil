using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class WorkSpaceManager : MonoBehaviour, IPointerDownHandler {
	public void OnPointerDown(PointerEventData eventData) {
		if(Input.GetMouseButtonDown(0))
			OnMouseLeftButtonDown();
		else if(Input.GetMouseButton(1)) OnMouseRightButtonDown();
	}

	private static void OnMouseLeftButtonDown() {
		if(KeyboardEventManager.GetControl() || KeyboardEventManager.GetShift()) return;
		List<string> removeElements = null;
		if(GlobalData.CurrentSelectDisplayObjectDic.Count > 0) removeElements = GlobalData.CurrentSelectDisplayObjectDic.KeyList();
		if(removeElements != null) HistoryManager.Do(BehaviorFactory.GetUpdateSelectDisplayObjectBehavior(GlobalData.CurrentModule, null, removeElements));
	}

	private static void OnMouseRightButtonDown() {
		Debug.Log("OnMouseRightDown");
	}
}
