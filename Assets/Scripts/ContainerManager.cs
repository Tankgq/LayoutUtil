using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ContainerManager : MonoBehaviour {
	public GameObject containerScrollView;
	public Text moduleNameText;
	public Text selectedDisplayObjectText;
	public Slider scaleSlider;

	private void Start() {
		GlobalData.GlobalObservable.ObserveEveryValueChanged(_ => GlobalData.CurrentModule)
				  .SampleFrame(1)
				  .Subscribe(module => {
					   DisplayObjectUtil.RemoveAllDisplayObjectBehavior();
					   DisplayObjectUtil.AddAllDisplayObjectBehavior();
					   MessageBroker.SendUpdateModuleTxtWidth();
					   if(string.IsNullOrEmpty(module)) {
						   moduleNameText.text = "null";
						   return;
					   }

					   moduleNameText.text = module;
					   GlobalData.CurrentSelectDisplayObjectDic.Clear();
					   scaleSlider.value = 10f;
					   GetComponent<RectTransform>().localPosition = Vector2.zero;
				   });
		Subject<object[]> updateModuleTxtWidthSubject = MessageBroker.GetSubject(MessageCode.UpdateModuleTxtWidth);
		updateModuleTxtWidthSubject.SampleFrame(1)
								   .DelayFrame(1)
								   .Subscribe(_ => {
										RectTransform rt = moduleNameText.GetComponent<RectTransform>();
										RectTransform rt2 = selectedDisplayObjectText.GetComponent<RectTransform>();
										rt2.anchoredPosition = new Vector2(rt.anchoredPosition.x + rt.sizeDelta.x + 30,
																		   rt2.anchoredPosition.y);
									});
		Subject<object[]> updateSelectDisplayObjectSubject = MessageBroker.GetSubject(MessageCode.UpdateSelectDisplayObjectDic);
		updateSelectDisplayObjectSubject.Subscribe(objects => {
			if(objects.Length == 0) return;
			if(objects.Length > 1 && objects[1] is List<string>) {
				List<string> removeElements = (List<string>)objects[1];
				foreach(Transform displayObject in removeElements.Select(elementName => GlobalData.CurrentDisplayObjectDic[elementName])
																 .Where(displayObject => displayObject)) {
					displayObject.GetComponent<Toggle>().isOn = false;
				}
			}

			if(objects.Length > 0 && objects[0] is List<string>) {
				List<string> addElements = (List<string>)objects[0];
				foreach(Transform displayObject in addElements.Select(elementName => GlobalData.CurrentDisplayObjectDic[elementName])
															  .Where(displayObject => displayObject)) {
					displayObject.GetComponent<Toggle>().isOn = true;
				}
			}

			if(GlobalData.CurrentSelectDisplayObjectDic.Count < 1) {
				selectedDisplayObjectText.text = "null";
				return;
			}

			StringBuilder sb = new StringBuilder();
			foreach(var pair in GlobalData.CurrentSelectDisplayObjectDic) {
				sb.Append($"{pair.Value.name}, ");
				pair.Value.GetComponent<Toggle>().isOn = true;
			}

			selectedDisplayObjectText.text = sb.ToString(0, sb.Length - 2);
		});
		GlobalData.GlobalObservable.ObserveEveryValueChanged(_ => GlobalData.ModifyDic)
				  .SampleFrame(1)
				  .Subscribe(modifyCount => MessageBroker.SendUpdateTitle());
		GlobalData.GlobalObservable.ObserveEveryValueChanged(_ => GlobalData.CurrentFilePath)
				  .SampleFrame(1)
				  .Subscribe(_ => MessageBroker.SendUpdateTitle());
		Subject<object[]> updateTitleSubject = MessageBroker.GetSubject(MessageCode.UpdateTitle);
		updateTitleSubject.SampleFrame(1)
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
		if(isControlDown)
			removeElements = new List<string>();
		else
			addElements = new List<string>();
		List<Element> elements = GlobalData.ModuleDic[GlobalData.CurrentModule];
		foreach(Element element in elements.Where(rect => rect.IsCrossing(selectRect)))
			if(isControlDown)
				removeElements.Add(element.Name);
			else
				addElements.Add(element.Name);
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
		List<string> copyNames = new List<string>();
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
			copyNames.Add(elementName);
		}
		HistoryManager.Do(BehaviorFactory.GetCopyDisplayObjectsBehavior(moduleName, copyNames), true);
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
