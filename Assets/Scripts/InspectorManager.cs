using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FarPlane;
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
				   .Select(_ => (float)xInputField.text.Calculate())
				   .Where(x => ! float.IsNaN(x))
				   .Subscribe(x => {
						int length = GlobalData.CurrentSelectDisplayObjectDic.Count;
						if(_displayObject && length == 1) {
							Element element = GlobalData.GetElement(_displayObject.name);
							if(element == null || Utils.IsEqual(element.X, x)) return;
							HistoryManager.Do(BehaviorFactory.GetChangeXBehavior(GlobalData.CurrentModule, new List<string> {element.Name}, element.X, x));
							return;
						}

						if(Utils.IsEqual(x, 0.0f) || length < 2) return;
						List<string> elementNames = GlobalData.CurrentSelectDisplayObjectDic.KeyList();
						HistoryManager.Do(BehaviorFactory.GetChangeXBehavior(GlobalData.CurrentModule, elementNames, -x, x, true));
						xInputField.text = "0";
					});

		yInputField.ObserveEveryValueChanged(element => element.isFocused)
				   .Where(isFocused => ! isFocused && ! string.IsNullOrEmpty(yInputField.text))
				   .Select(_ => (float)yInputField.text.Calculate())
				   .Where(y => ! float.IsNaN(y))
				   .Subscribe(y => {
						int length = GlobalData.CurrentSelectDisplayObjectDic.Count;
						if(_displayObject && length == 1) {
							Element element = GlobalData.GetElement(_displayObject.name);
							if(element == null || Utils.IsEqual(element.Y, y)) return;
							HistoryManager.Do(BehaviorFactory.GetChangeYBehavior(GlobalData.CurrentModule, new List<string> {element.Name}, element.Y, y));
							return;
						}

						if(Utils.IsEqual(y, 0.0f) || length < 2) return;
						List<string> elementNames = GlobalData.CurrentSelectDisplayObjectDic.KeyList();
						HistoryManager.Do(BehaviorFactory.GetChangeYBehavior(GlobalData.CurrentModule, elementNames, y, -y, true));
						yInputField.text = "0";
					});

		widthInputField.ObserveEveryValueChanged(element => element.isFocused)
					   .Where(isFocused => ! isFocused && ! string.IsNullOrEmpty(widthInputField.text))
					   .Select(_ => (float)widthInputField.text.Calculate())
					   .Where(width => ! float.IsNaN(width))
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
							List<string> elementNames = GlobalData.CurrentSelectDisplayObjectDic.KeyList();
							HistoryManager.Do(BehaviorFactory.GetChangeWidthBehavior(GlobalData.CurrentModule, elementNames, -width, width, true));
							widthInputField.text = "0";
						});
		heightInputField.ObserveEveryValueChanged(element => element.isFocused)
						.Where(isFocused => ! isFocused && ! string.IsNullOrEmpty(heightInputField.text))
						.Select(_ => (float)heightInputField.text.Calculate())
						.Where(height => ! float.IsNaN(height))
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
							 List<string> elementNames = GlobalData.CurrentSelectDisplayObjectDic.KeyList();
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
		UlEventSystem.GetSubject<DataEventType, SelectedChangeData>(DataEventType.SelectedChange)
//					 .SampleFrame(1)
					 .Subscribe(_ => {
						  Transform displayObject = GlobalData.CurrentSelectDisplayObjectDic.OnlyValue();
						  UpdateState(displayObject);
					  });
		UlEventSystem.GetTriggerSubject<UIEventType>(UIEventType.UpdateInspectorInfo)
					 .SampleFrame(1)
					 .Subscribe(_ => UpdateState(_displayObject));
	}

	private static float ParseFloat(string txt) {
		bool bSucceed = float.TryParse(txt, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out float result);
		return bSucceed ? result : GlobalData.MinFloat;
	}

	private void UpdateState(Transform displayObject) {
		bool enabled = GlobalData.CurrentSelectDisplayObjectDic.Count > 0;
		_displayObject = displayObject;
		nameInputField.enabled = ! string.IsNullOrWhiteSpace(GlobalData.CurrentModule);
		xInputField.enabled = enabled;
		yInputField.enabled = enabled;
		widthInputField.enabled = enabled;
		heightInputField.enabled = enabled;
		if(_displayObject == null) {
			nameInputField.text = string.IsNullOrWhiteSpace(GlobalData.CurrentModule) ? "null" : GlobalData.CurrentModule;
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

	/**
	 * 简单计算一个只有加减法的表达式, 不考虑括号
	 */
	private static float CalculateSimpleExpression(string expression) {
		expression = expression.Trim();
		if(expression.Length == 0) return GlobalData.MinFloat;
		Queue<char> operatorCharQueue = new Queue<char>();
		Queue<float> numberQueue = new Queue<float>();
		if(expression[0] == '+' || expression[0] == '-') numberQueue.Enqueue(0.0f);
		int length = expression.Length;
		int start = -1, dotCount = 0;
		for(int idx = 0; idx < length; ++ idx) {
			if(expression[idx] == '+' || expression[idx] == '-') {
				operatorCharQueue.Enqueue(expression[idx]);
				if(start != -1) {
					numberQueue.Enqueue(ParseFloat(expression.Substring(start, idx - start)));
					dotCount = 0;
					start = -1;
				}
				continue;
			}
			if(! char.IsDigit(expression[idx]) && expression[idx] != '.') return GlobalData.MinFloat;
			if(expression[idx] == '.') {
				++ dotCount;
				if(dotCount > 1) return GlobalData.MinFloat;
			}
			if(start == -1)
				start = idx;
		}
		if(start != -1) numberQueue.Enqueue(ParseFloat(expression.Substring(start, length - start)));
		if(numberQueue.Count == 0) return GlobalData.MinFloat;
		float result = numberQueue.Dequeue();
		while(numberQueue.Count > 0) {
			if(result < GlobalData.MinFloat) result = numberQueue.Dequeue();
			if(operatorCharQueue.Count == 0) return GlobalData.MinFloat;
			if(numberQueue.Count == 0) return result;
			float number = numberQueue.Dequeue();
			char operatorChar = operatorCharQueue.Dequeue();
			if(operatorChar == '+') result += number;
			else if(operatorChar == '-') result -= number;
			else return GlobalData.MinFloat;
		}
		return operatorCharQueue.Count != 0 ? GlobalData.MinFloat : result;
	}
}
