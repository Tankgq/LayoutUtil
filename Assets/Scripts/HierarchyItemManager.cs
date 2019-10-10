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
			GlobalData.CurrentModule = module;
			HistoryManager.Do(BehaviorFactory.GetOpenModuleBehavior(module, true));
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
			if(! KeyboardEventManager.GetShift() && GlobalData.CurrentSelectDisplayObjectDic.Count > 0)
				removeElements = GlobalData.CurrentSelectDisplayObjectDic.Select(pair => pair.Key).ToList();
			addElements = new List<string> {elementName};
		}
		if(addElements != null || removeElements != null)
			HistoryManager.Do(BehaviorFactory.GetUpdateSelectDisplayObjectBehavior(GlobalData.CurrentModule, addElements, removeElements));
	}
}
