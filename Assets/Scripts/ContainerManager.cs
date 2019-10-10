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
		Subject<object[]> updateModuleTxtWidthSubject = MessageBroker.GetSubject(MessageBroker.Code.UpdateModuleTxtWidth);
		updateModuleTxtWidthSubject.SampleFrame(1)
								   .DelayFrame(1)
								   .Subscribe(_ => {
										RectTransform rt = moduleNameText.GetComponent<RectTransform>();
										RectTransform rt2 = selectedDisplayObjectText.GetComponent<RectTransform>();
										rt2.anchoredPosition = new Vector2(rt.anchoredPosition.x + rt.sizeDelta.x + 30,
																		   rt2.anchoredPosition.y);
									});
		Subject<object[]> updateSelectDisplayObjectSubject = MessageBroker.GetSubject(MessageBroker.Code.UpdateSelectDisplayObjectDic);
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

			if(GlobalData.CurrentSelectDisplayObjectDic.Count < 1) return;
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
		Subject<object[]> updateTitleSubject = MessageBroker.GetSubject(MessageBroker.Code.UpdateTitle);
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
		if(string.IsNullOrEmpty(GlobalData.CurrentModule)) return;
		foreach(Element displayObject in GlobalData.ModuleDic[GlobalData.CurrentModule].Where(displayObject => displayObject.IsCrossing(selectRect)))
			if(KeyboardEventManager.GetControl())
				GlobalData.CurrentSelectDisplayObjectDic.Remove(displayObject.Name);
			else
				GlobalData.CurrentSelectDisplayObjectDic[displayObject.Name] = GlobalData.CurrentDisplayObjectDic[displayObject.Name];
	}

	public void PasteDisplayObjects() {
		if(GlobalData.CurrentCopyDisplayObjects.Count == 0) return;
		List<Element> copyList = GlobalData.CurrentCopyDisplayObjects;
		Vector2 leftTop = DisplayObjectUtil.GetCopyDisplayObjectsLeftTop(copyList);
		Vector2 mousePos = Element.InvConvertTo(GlobalData.OriginPoint);
		if(Utils.IsPointOverGameObject(containerScrollView)) mousePos = Utils.GetRealPositionInContainer(Input.mousePosition);
		int count = copyList.Count;
		for(int idx = 0; idx < count; ++ idx) {
			Vector2 pos = mousePos - leftTop;
			pos.x += copyList[idx].X;
			pos.y += copyList[idx].Y;
			DisplayObjectUtil.AddDisplayObject(null, pos, new Vector2(copyList[idx].Width, copyList[idx].Height), copyList[idx].Name);
		}
	}

	public static bool CheckPointOnAnyDisplayObject() {
		if(string.IsNullOrEmpty(GlobalData.CurrentModule)) return false;
		Vector2 pos = Element.ConvertTo(Utils.GetAnchoredPositionInContainer(Input.mousePosition));
		return GlobalData.ModuleDic[GlobalData.CurrentModule].Any(displayObject => displayObject.Contain(pos));
	}

	public static void UpdateCurrentDisplayObjectData() {
		if(string.IsNullOrEmpty(GlobalData.CurrentModule)) return;
		List<Element> displayObjectDataList = GlobalData.ModuleDic[GlobalData.CurrentModule];
		int count = GlobalData.CurrentDisplayObjects.Count;
		for(int idx = 0; idx < count; ++ idx) {
			Transform displayObject = GlobalData.CurrentDisplayObjects[idx];
			Element displayObjectData = displayObjectDataList[idx];
			displayObjectData.Name = displayObject.name;
			RectTransform rt = displayObject.GetComponent<RectTransform>();
			Vector2 pos = rt.anchoredPosition;
			Vector2 size = rt.sizeDelta;
			displayObjectData.X = Element.ConvertX(pos.x);
			displayObjectData.Y = Element.ConvertY(pos.y);
			displayObjectData.Width = size.x;
			displayObjectData.Height = size.y;
		}
	}
}
