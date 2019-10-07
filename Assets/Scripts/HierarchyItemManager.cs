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
			GlobalData.CurrentSelectDisplayObjectDic.Add(elementName, GlobalData.CurrentDisplayObjectDic[elementName]);
			MessageBroker.SendUpdateSelectDisplayObjectDic();
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
		if(isSelect) {
			if(KeyboardEventManager.GetControl()) {
				GlobalData.CurrentSelectDisplayObjectDic.Remove(elementName);
			}
		} else {
			if(! KeyboardEventManager.GetShift()) DeselectAllDisplayObjectItem();
			GlobalData.CurrentSelectDisplayObjectDic.Add(elementName, displayObject);
			MessageBroker.SendUpdateSelectDisplayObjectDic();
		}
	}

	public static void DeselectAllDisplayObjectItem() {
		GlobalData.CurrentSelectDisplayObjectDic.Clear();
	}
}
