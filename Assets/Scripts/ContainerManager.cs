using System.Collections.Generic;
using System.Linq;
using System.Text;
using FarPlane;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ContainerManager : MonoBehaviour {
	public GameObject containerScrollView;
	public Text moduleNameText;
	public Text selectedDisplayObjectText;
	public Slider scaleSlider;

	private void Start() {
		UlEventSystem.GetSubject<DataEventType, ChangeModuleEventData>(DataEventType.ChangeModule)
					 .Subscribe(eventData => {
						  if(eventData == null) return;
						  string module = eventData.Module;
						  if(string.IsNullOrWhiteSpace(module)) module = null;
						  if(GlobalData.CurrentModule == module) return;
						  List<string> removeElements = GlobalData.CurrentSelectDisplayObjectDic.KeyList();
						  if(removeElements != null)
							  HistoryManager.Do(BehaviorFactory.GetUpdateSelectDisplayObjectBehavior(GlobalData.CurrentModule,
																									 null,
																									 removeElements,
																									 CombineType.Previous));
						  GlobalData.CurrentModule = module;
						  DisplayObjectUtil.RemoveAllDisplayObjectBehavior();
						  DisplayObjectUtil.AddAllDisplayObjectBehavior();
						  UlEventSystem.DispatchTrigger<DataEventType>(DataEventType.CurrentModuleNameChanged);
						  GetComponent<RectTransform>().localPosition = Vector2.zero;
						  scaleSlider.value = 10f;
					  });
		UlEventSystem.GetTriggerSubject<DataEventType>(DataEventType.CurrentModuleNameChanged)
					 .Subscribe(_ => {
						  UlEventSystem.DispatchTrigger<UIEventType>(UIEventType.UpdateModuleTxtWidth);
						  moduleNameText.text = string.IsNullOrWhiteSpace(GlobalData.CurrentModule) ? "null" : GlobalData.CurrentModule;
					  });
		UlEventSystem.GetSubject<UIEventType, TriggerEventData>(UIEventType.UpdateModuleTxtWidth)
					 .SampleFrame(1)
					 .DelayFrame(0)
					 .Subscribe(_ => {
						  RectTransform rt = moduleNameText.GetComponent<RectTransform>();
						  RectTransform rt2 = selectedDisplayObjectText.GetComponent<RectTransform>();
						  rt2.anchoredPosition = new Vector2(rt.anchoredPosition.x + rt.sizeDelta.x + 30, rt2.anchoredPosition.y);
					  });
		UlEventSystem.GetSubject<DataEventType, SelectedChangeData>(DataEventType.SelectedChange)
					 .Subscribe(eventData => {
						  if(eventData == null
						  || string.IsNullOrWhiteSpace(eventData.ModuleName)
						  || ! eventData.ModuleName.Equals(GlobalData.CurrentModule))
							  return;
						  if(eventData.RemoveElements != null) {
							  foreach(Transform displayObject in eventData.RemoveElements
																		  .Select(elementName => {
																			   GlobalData.CurrentDisplayObjectDic.TryGetValue(elementName, out Transform displayObject);
																			   return displayObject;
																		   })
																		  .Where(displayObject => displayObject)) {
								  displayObject.GetComponent<FrameManager>().IsSelect = false;
							  }
						  }

						  if(eventData.AddElements != null) {
							  foreach(Transform displayObject in eventData.AddElements
																		  .Select(elementName => GlobalData.CurrentDisplayObjectDic[elementName])
																		  .Where(displayObject => displayObject)) {
								  displayObject.GetComponent<FrameManager>().IsSelect = true;
							  }
						  }

						  if(GlobalData.CurrentSelectDisplayObjectDic.Count < 1) {
							  selectedDisplayObjectText.text = "null";
							  return;
						  }

						  StringBuilder sb = new StringBuilder();
						  foreach(var pair in GlobalData.CurrentSelectDisplayObjectDic) {
							  sb.Append($"{pair.Value.name}, ");
							  pair.Value.GetComponent<FrameManager>().IsSelect = true;
						  }

						  selectedDisplayObjectText.text = sb.ToString(0, sb.Length - 2);
					  });
		GlobalData.GlobalObservable.ObserveEveryValueChanged(_ => GlobalData.ModifyDic)
				  .SampleFrame(1)
				  .Subscribe(modifyCount => UlEventSystem.DispatchTrigger<UIEventType>(UIEventType.UpdateTitle));
		GlobalData.GlobalObservable.ObserveEveryValueChanged(_ => GlobalData.CurrentFilePath)
				  .SampleFrame(1)
				  .Subscribe(_ => UlEventSystem.DispatchTrigger<UIEventType>(UIEventType.UpdateTitle));
		UlEventSystem.GetSubject<UIEventType, TriggerEventData>(UIEventType.UpdateTitle)
					 .SampleFrame(1)
					 .Subscribe(_ => {
						  string title = GlobalData.ProductName;
						  if(! string.IsNullOrWhiteSpace(GlobalData.CurrentFilePath)) title = GlobalData.CurrentFilePath;
						  Utils.ChangeTitle(GlobalData.ModifyCount != 0 ? $"* {title}" : title);
					  });
	}

	public static void RemoveSelectedDisplayObjectOrModules() {
		if(GlobalData.CurrentSelectDisplayObjectDic.Count > 0)
			DisplayObjectUtil.RemoveSelectedDisplayObject();
		else
			ModuleUtil.CheckRemoveCurrentModule();
	}

	public static void SelectDisplayObjectsInDisplayObject(Rectangle selectRect) {
		if(string.IsNullOrWhiteSpace(GlobalData.CurrentModule)) return;
		bool isControlDown = KeyboardEventManager.GetControl();
		List<string> addElements = null, removeElements = null;
		if(isControlDown) removeElements = new List<string>();
		else addElements = new List<string>();
		List<Element> elements = GlobalData.ModuleDic[GlobalData.CurrentModule];
		foreach(Element element in elements.Where(rect => rect.IsCrossing(selectRect))) {
			if(isControlDown) removeElements.Add(element.Name);
			else addElements.Add(element.Name);
		}
		if(isControlDown && removeElements.Count == 0) return;
		if(! isControlDown && addElements.Count == 0) return;
		HistoryManager.Do(BehaviorFactory.GetUpdateSelectDisplayObjectBehavior(GlobalData.CurrentModule, addElements, removeElements));
	}

	public void PasteDisplayObjects() {
		if(GlobalData.CurrentCopyDisplayObjects.Count == 0) return;
		List<Element> sourceList = GlobalData.CurrentCopyDisplayObjects;
		Vector2 leftTop = DisplayObjectUtil.GetCopyDisplayObjectsLeftTop(sourceList);
		Vector2 mousePos = Vector2.zero;
		if(Utils.IsPointOverGameObject(containerScrollView)) mousePos = Utils.GetRealPosition(Input.mousePosition);
		Vector2 delta = mousePos - leftTop;
		int count = sourceList.Count;
		string moduleName = GlobalData.CurrentModule;
		List<Element> copiedElements = new List<Element>();
		for(int idx = 0; idx < count; ++ idx) {
			Element sourceElement = sourceList[idx];
			string imageUrl = DisplayObjectUtil.GetImageUrl(moduleName, sourceElement.Name);
			string elementName = DisplayObjectUtil.GetCanUseElementName(sourceElement.Name, imageUrl);
			Vector2 pos = new Vector2(sourceElement.X + delta.x, sourceElement.Y + delta.y);
			Vector2 size = new Vector2(sourceElement.Width, sourceElement.Height);
			Element element = new Element {
				Name = elementName,
				X = pos.x,
				Y = pos.y,
				Width = size.x,
				Height = size.y,
				Visible = true
			};
			DisplayObjectUtil.AddDisplayObjectBehavior(moduleName, element, imageUrl);
			copiedElements.Add(element);
		}

		HistoryManager.Do(BehaviorFactory.GetCopyDisplayObjectsBehavior(moduleName, copiedElements), true);
	}

	public static bool CheckPointOnAnyDisplayObject() {
		if(string.IsNullOrEmpty(GlobalData.CurrentModule)) return false;
		Vector2 pos = Element.ConvertTo(Utils.GetAnchoredPositionInContainer(Input.mousePosition));
		return GlobalData.ModuleDic[GlobalData.CurrentModule].Any(displayObject => displayObject.Contain(pos));
	}

	public static void UpdateCurrentDisplayObjectData() {
		if(string.IsNullOrEmpty(GlobalData.CurrentModule)) return;
		List<Element> elements = GlobalData.ModuleDic[GlobalData.CurrentModule];
		int count = GlobalData.CurrentDisplayObjects.Count;
		for(int idx = 0; idx < count; ++ idx) {
			Transform displayObject = GlobalData.CurrentDisplayObjects[idx];
			Element element = elements[idx];
			element.Name = displayObject.name;
			RectTransform rt = displayObject.GetComponent<RectTransform>();
			Vector2 pos = rt.anchoredPosition;
			Vector2 size = rt.sizeDelta;
			element.X = Element.ConvertX(pos.x);
			element.Y = Element.ConvertY(pos.y);
			element.Width = size.x;
			element.Height = size.y;
		}
	}
}
