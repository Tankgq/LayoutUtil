using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InspectorManager : MonoBehaviour {
	private Transform _displayObject;

	public InputField nameInputField;
	public InputField xInputField;
	public InputField yInputField;
	public InputField widthInputField;
	public InputField heightInputField;

	private void Start() {
		// EventSystem.current.SetSelectedGameObject(NameInputField.gameObject);
		nameInputField.ObserveEveryValueChanged(element => element.isFocused)
					  .Where(isFocus => ! string.IsNullOrWhiteSpace(GlobalData.CurrentModule)
									 && _displayObject
									 && ! isFocus
									 && ! string.IsNullOrEmpty(nameInputField.text))
					  .Subscribe(_ => {
						   string newName = nameInputField.text.Trim();
						   string originName = _displayObject.name;
						   if(newName.Equals(originName)) return;
						   if(GlobalData.CurrentDisplayObjectDic.ContainsKey(newName)) {
							   DialogManager.ShowError("该名称已存在", 0, 0);
							   nameInputField.text = originName;
							   return;
						   }
						   HistoryManager.Do(BehaviorFactory.GetChangeNameBehavior(GlobalData.CurrentModule, originName, newName));
					   });
		xInputField.ObserveEveryValueChanged(element => element.isFocused)
				   .Where(isFocused => ! isFocused && ! string.IsNullOrEmpty(xInputField.text))
				   .Select(_ => ParseFloat(xInputField.text))
				   .Where(x => x > GlobalData.MinFloat)
				   .Subscribe(x => {
						int length = GlobalData.CurrentSelectDisplayObjectDic.Count;
						if(_displayObject && length == 1) {
							Element element = GlobalData.GetElement(_displayObject.name);
							if(element == null || Utils.IsEqual(element.X, x)) return;
							HistoryManager.Do(BehaviorFactory.GetChangeXBehavior(GlobalData.CurrentModule, new List<string> {element.Name}, element.X, x));
							return;
						}
						if(Utils.IsEqual(x, 0.0f) || length < 2) return;
						List<string> elementNames = GlobalData.CurrentSelectDisplayObjectDic.Select(pair => pair.Key).ToList();
						HistoryManager.Do(BehaviorFactory.GetChangeXBehavior(GlobalData.CurrentModule, elementNames, -x, x, true));
						xInputField.text = "0";
					});

		yInputField.ObserveEveryValueChanged(element => element.isFocused)
				   .Where(isFocused => ! isFocused && ! string.IsNullOrEmpty(yInputField.text))
				   .Select(_ => ParseFloat(yInputField.text))
				   .Where(y => y > GlobalData.MinFloat)
				   .Subscribe(y => {
						int length = GlobalData.CurrentSelectDisplayObjectDic.Count;
						if(_displayObject && length == 1) {
							Element element = GlobalData.GetElement(_displayObject.name);
							if(element == null || Utils.IsEqual(element.Y, y)) return;
							HistoryManager.Do(BehaviorFactory.GetChangeYBehavior(GlobalData.CurrentModule, new List<string> {element.Name}, element.Y, y));
							return;
						}
						if(Utils.IsEqual(y, 0.0f) || length < 2) return;
						List<string> elementNames = GlobalData.CurrentSelectDisplayObjectDic.Select(pair => pair.Key).ToList();
						HistoryManager.Do(BehaviorFactory.GetChangeYBehavior(GlobalData.CurrentModule, elementNames, -y, y, true));
						yInputField.text = "0";
					});

		widthInputField.ObserveEveryValueChanged(element => element.isFocused)
					   .Where(isFocused => ! isFocused && ! string.IsNullOrEmpty(widthInputField.text))
					   .Select(_ => ParseFloat(widthInputField.text))
					   .Where(width => width > GlobalData.MinFloat)
					   .Subscribe(width => {
							int length = GlobalData.CurrentSelectDisplayObjectDic.Count;
							if(_displayObject && length == 1) {
								Element element = GlobalData.GetElement(_displayObject.name);
								if(element == null || Utils.IsEqual(element.Width, width)) return;
								HistoryManager.Do(BehaviorFactory.GetChangeWidthBehavior(GlobalData.CurrentModule,
																						 new List<string> {element.Name},
																						 element.Width,
																						 width));
								return;
							}
							if(Utils.IsEqual(width, 0.0f) || length < 2) return;
							List<string> elementNames = GlobalData.CurrentSelectDisplayObjectDic.Select(pair => pair.Key).ToList();
							HistoryManager.Do(BehaviorFactory.GetChangeWidthBehavior(GlobalData.CurrentModule, elementNames, -width, width, true));
							widthInputField.text = "0";
						});
		heightInputField.ObserveEveryValueChanged(element => element.isFocused)
						.Where(isFocused => ! isFocused && ! string.IsNullOrEmpty(heightInputField.text))
						.Select(_ => ParseFloat(heightInputField.text))
						.Where(height => height > GlobalData.MinFloat)
						.Subscribe(height => {
							 int length = GlobalData.CurrentSelectDisplayObjectDic.Count;
							 if(_displayObject && length == 1) {
								 Element element = GlobalData.GetElement(_displayObject.name);
								 if(element == null || Utils.IsEqual(element.Height, height)) return;
								 HistoryManager.Do(BehaviorFactory.GetChangeHeightBehavior(GlobalData.CurrentModule,
																						  new List<string> {element.Name},
																						  element.Height,
																						  height));
								 return;
							 }
							 if(Utils.IsEqual(height, 0.0f) || length < 2) return;
							 List<string> elementNames = GlobalData.CurrentSelectDisplayObjectDic.Select(pair => pair.Key).ToList();
							 HistoryManager.Do(BehaviorFactory.GetChangeHeightBehavior(GlobalData.CurrentModule, elementNames, -height, height, true));
							 heightInputField.text = "0";
						 });
		Observable.EveryUpdate()
				  .Where(_ => EventSystem.current.currentSelectedGameObject != null && (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Return)))
				  .Subscribe(_ => {
					   GameObject go = EventSystem.current.currentSelectedGameObject;

					   bool isShiftDown = KeyboardEventManager.GetShift();
					   if((isShiftDown && go == yInputField.gameObject) || (! isShiftDown && go == nameInputField.gameObject))
						   EventSystem.current.SetSelectedGameObject(xInputField.gameObject);
					   else if((isShiftDown && go == widthInputField.gameObject) || (! isShiftDown && go == xInputField.gameObject))
						   EventSystem.current.SetSelectedGameObject(yInputField.gameObject);
					   else if((isShiftDown && go == heightInputField.gameObject) || (! isShiftDown && go == yInputField.gameObject))
						   EventSystem.current.SetSelectedGameObject(widthInputField.gameObject);
					   else if((isShiftDown && go == nameInputField.gameObject) || (! isShiftDown && go == widthInputField.gameObject))
						   EventSystem.current.SetSelectedGameObject(heightInputField.gameObject);
					   else if((isShiftDown && go == xInputField.gameObject) || (! isShiftDown && go == heightInputField.gameObject))
						   EventSystem.current.SetSelectedGameObject(nameInputField.gameObject);
				   });
		Subject<object[]> updateDisplayObjectSubject = MessageBroker.GetSubject(MessageBroker.Code.UpdateSelectDisplayObjectDic);
		updateDisplayObjectSubject.SampleFrame(1)
								  .Subscribe(_ => {
									   UpdateState(GlobalData.CurrentSelectDisplayObjectDic.Count != 1 ? null : GlobalData.CurrentSelectDisplayObjectDic.First().Value);
								   });
		Subject<object[]> updateDisplayObjectPosSubject =
				MessageBroker.GetSubject(MessageBroker.Code.UpdateDisplayObjectPos);
		updateDisplayObjectPosSubject.SampleFrame(1)
									 .Subscribe(_ => UpdateState(_displayObject));
	}

	private static float ParseFloat(string txt) {
		float result;
		var bSucceed = float.TryParse(txt, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out result);
		return bSucceed ? result : GlobalData.MinFloat;
	}

	private void UpdateState(Transform displayObject) {
		_displayObject = displayObject;
		if(_displayObject == null) {
			nameInputField.text = "null";
			xInputField.text = "0";
			yInputField.text = "0";
			widthInputField.text = "0";
			heightInputField.text = "0";
			return;
		}
		Element element = Element.ConvertTo(_displayObject);
		nameInputField.text = element.Name;
		xInputField.text = $"{element.X:F1}";
		yInputField.text = $"{element.Y:F1}";
		widthInputField.text = $"{element.Width:F1}";
		heightInputField.text = $"{element.Height:F1}";
	}
}
