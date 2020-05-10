using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class HierarchyItemManager : MonoBehaviour, IPointerDownHandler {
	public int itemType;

	public void OnPointerDown(PointerEventData eventData) {
		string elementName = Utils.CancelHighlight(transform.name);
		if(itemType == 1) {
			ContainerManager.UpdateCurrentDisplayObjectData();
			string moduleName = elementName;
			if(moduleName.Equals(GlobalData.CurrentModule)) moduleName = null;
			ModuleUtil.OpenModule(moduleName);
			return;
		}
		if(itemType != 2) return;
		if(HierarchyManager.InSearchMode()) {
			string module = HierarchyManager.GetModuleName(transform.GetSiblingIndex());
			if(string.IsNullOrEmpty(module)) return;
			HistoryManager.Do(BehaviorFactory.GetOpenModuleBehavior(module, CombineType.Next));
			HistoryManager.Do(BehaviorFactory.GetUpdateSelectDisplayObjectBehavior(module, new List<string>{elementName}));
			return;
		}
		if(string.IsNullOrEmpty(GlobalData.CurrentModule)) return;
		Transform displayObject = GlobalData.CurrentDisplayObjectDic[elementName];
		if(displayObject.parent == null) return;
		SwapImageManager sim = transform.GetComponentInChildren<SwapImageManager>();
		if(sim && Utils.IsPointOverGameObject(sim.gameObject)) {
			HistoryManager.Do(BehaviorFactory.GetUpdateSwapImageBehavior(GlobalData.CurrentModule, elementName, ! sim.isSwap));
			return;
		}
		bool isSelect = GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(elementName);
		List<string> addElements = null, removeElements = null; 
		if(isSelect) {
			if(KeyboardEventManager.GetControl()) {
				removeElements = new List<string> {elementName};
			}
		} else {
			if(! KeyboardEventManager.GetShift())
				removeElements = GlobalData.CurrentSelectDisplayObjectDic.KeyList();
			addElements = new List<string> {elementName};
		}
		if(addElements != null || removeElements != null)
			HistoryManager.Do(BehaviorFactory.GetUpdateSelectDisplayObjectBehavior(GlobalData.CurrentModule, addElements, removeElements));
	}
}
