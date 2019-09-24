using System;
using System.Globalization;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InspectorManager : MonoBehaviour
{
	private Transform _displayObject;
	private IDisposable _disposable;

	public InputField nameInputField;
	public InputField xInputField;
	public InputField yInputField;
	public InputField widthInputField;
	public InputField heightInputField;

	private void Start()
	{
		// EventSystem.current.SetSelectedGameObject(NameInputField.gameObject);
		nameInputField.ObserveEveryValueChanged(element => element.isFocused)
					  .Where(isFocus => !string.IsNullOrWhiteSpace(GlobalData.CurrentModule) && _displayObject
																							 && !isFocus && !string.IsNullOrEmpty(nameInputField.text))
					  .Subscribe(_ =>
					  {
						  string newName = nameInputField.text.Trim();
						  string originName = _displayObject.name;
						  if (newName.Equals(originName)) return;
						  if (GlobalData.CurrentDisplayObjectDic.ContainsKey(newName))
						  {
							  DialogManager.ShowError("该名称已存在", 0, 0);
							  nameInputField.text = originName;
							  return;
						  }
						  new Action<string, string, string>((module, originName1, newName1) =>
						  {
							  HistoryManager.Do(new Behavior((isRedo) => ChangeNameBehavior(module, originName1, newName1, true),
															 (isReUndo) => ChangeNameBehavior(module, newName1, originName1, false)));
						  })(GlobalData.CurrentModule, originName, newName);
					  });
		xInputField.ObserveEveryValueChanged(element => element.isFocused)
				   .Where(isFocused => !isFocused && !string.IsNullOrEmpty(xInputField.text))
				   .Select(_ => ParseFloat(xInputField.text))
				   .Where(x => x > GlobalData.MinFloat)
				   .Subscribe(x =>
				   {
					   if (_displayObject)
					   {
						   Element element = GlobalData.GetElement(_displayObject.name);
						   if (element == null || Utils.IsEqual(element.X, x)) return;
						   new Action<string, string, float, float>((module, elementName, newX, originX) =>
						   {
							   HistoryManager.Do(new Behavior((isRedo) => ChangeXBehavior(module, elementName, newX, false, true),
															  (isReUndo) => ChangeXBehavior(module, elementName, originX, false, false)));
						   })(GlobalData.CurrentModule, _displayObject.name, x, element.X);
						   return;
					   }
					   if (Utils.IsEqual(x, 0.0f)) return;
					   if (GlobalData.CurrentSelectDisplayObjectDic.Count > 1)
						   foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic)
						   {
							   new Action<string, string, float, bool>((module, elementName, offsetX, isAdd) =>
							   {
								   HistoryManager.Do(new Behavior((isRedo) => ChangeXBehavior(module, elementName, offsetX, isAdd, true),
																  (isReUndo) => ChangeXBehavior(module, elementName, -offsetX, isAdd, false)));
							   })(GlobalData.CurrentModule, pair.Key, x, true);
						   }
					   xInputField.text = "0";
				   });
		yInputField.ObserveEveryValueChanged(element => element.isFocused)
				   .Where(isFocused => !isFocused && !string.IsNullOrEmpty(yInputField.text))
				   .Select(_ => ParseFloat(yInputField.text))
				   .Where(y => y > GlobalData.MinFloat)
				   .Subscribe(y =>
				   {
					   if (_displayObject)
					   {
						   Element element = GlobalData.GetElement(_displayObject.name);
						   if (element == null || Utils.IsEqual(element.Y, y)) return;
						   new Action<string, string, float, float>((module, elementName, newY, originY) =>
						   {
							   HistoryManager.Do(new Behavior((isRedo) => ChangeYBehavior(module, elementName, newY, false, true),
															  (isReUndo) => ChangeYBehavior(module, elementName, originY, false, false)));
						   })(GlobalData.CurrentModule, _displayObject.name, y, element.Y);
						   return;
					   }
					   if (Utils.IsEqual(y, 0.0f)) return;
					   if (GlobalData.CurrentSelectDisplayObjectDic.Count > 1)
						   foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic)
							   new Action<string, string, float, bool>((module, elementName, offsetY, isAdd) =>
							   {
								   HistoryManager.Do(new Behavior(
									   (isRedo) => ChangeYBehavior(module, elementName, offsetY, isAdd, true),
									   (isReUndo) => ChangeYBehavior(module, elementName, -offsetY, isAdd, false)));
							   })(GlobalData.CurrentModule, pair.Key, y, true);
					   yInputField.text = "0";
				   });
		widthInputField.ObserveEveryValueChanged(element => element.isFocused)
					   .Where(isFocused => !isFocused && !string.IsNullOrEmpty(widthInputField.text))
					   .Select(_ => ParseFloat(widthInputField.text))
					   .Where(width => width > GlobalData.MinFloat)
					   .Subscribe(width =>
					   {
						   if (_displayObject)
						   {
							   Element element = GlobalData.GetElement(_displayObject.name);
							   if (element == null || Utils.IsEqual(element.Width, width)) return;
							   new Action<string, string, float, float>((module, elementName, newWidth, originWidth) =>
							   {
								   HistoryManager.Do(new Behavior((isRedo) => ChangeWidthBehavior(module, elementName, newWidth, false, true),
																  (isReUndo) => ChangeWidthBehavior(module, elementName, originWidth, false, false)));
								   })(GlobalData.CurrentModule, _displayObject.name, width, element.Width);
								   return;
						   }
						   if (Utils.IsEqual(width, 0.0f)) return;
						   if (GlobalData.CurrentSelectDisplayObjectDic.Count > 1)
							   foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic)
								   new Action<string, string, float, bool>((module, elementName, newWidth, isAdd) =>
								   {
									   HistoryManager.Do(new Behavior((isRedo) => ChangeWidthBehavior(module, elementName, newWidth, isAdd, true),
										   (isReUndo) => ChangeWidthBehavior(module, elementName, -newWidth, isAdd, false)));
								   })(GlobalData.CurrentModule, pair.Key, width, true);
						   widthInputField.text = "0";
					   });
		heightInputField.ObserveEveryValueChanged(element => element.isFocused)
						.Where(isFocused => !isFocused && !string.IsNullOrEmpty(heightInputField.text))
						.Select(_ => ParseFloat(heightInputField.text))
						.Where(height => height > GlobalData.MinFloat)
						.Subscribe(height =>
						{
							if (_displayObject)
							{
								Element element = GlobalData.GetElement(_displayObject.name);
								if (element == null || Utils.IsEqual(element.Height, height)) return;
								new Action<string, string, float, float>((module, elementName, newHeight, originHeight) =>
								{
									HistoryManager.Do(new Behavior((isRedo) => ChangeHeightBehavior(module, elementName, newHeight, false, true),
										(isReUndo) => ChangeHeightBehavior(module, elementName, originHeight, false, false)));
								})(GlobalData.CurrentModule, _displayObject.name, height, element.Height);
								return;
							}
							if (Utils.IsEqual(height, 0.0f)) return;
							if (GlobalData.CurrentSelectDisplayObjectDic.Count > 1)
								foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic)
									new Action<string, string, float, bool>((module, elementName, newHeight, isAdd) =>
									{
										HistoryManager.Do(new Behavior((isRedo) => ChangeHeightBehavior(module, elementName, newHeight, isAdd, true),
											(isReUndo) => ChangeHeightBehavior(module, elementName, -newHeight, isAdd, false)));
									})(GlobalData.CurrentModule, pair.Key, height, true);
							heightInputField.text = "0";
						});
		Observable.EveryUpdate()
				  .Where(_ => EventSystem.current.currentSelectedGameObject != null && (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Return)))
				  .Subscribe(_ =>
				  {
					  GameObject go = EventSystem.current.currentSelectedGameObject;
					  bool isShiftDown = KeyboardEventManager.GetShift();
					  if ((isShiftDown && go == yInputField.gameObject) || (!isShiftDown && go == nameInputField.gameObject))
						  EventSystem.current.SetSelectedGameObject(xInputField.gameObject);
					  else if ((isShiftDown && go == widthInputField.gameObject) || (!isShiftDown && go == xInputField.gameObject))
						  EventSystem.current.SetSelectedGameObject(yInputField.gameObject);
					  else if ((isShiftDown && go == heightInputField.gameObject) || (!isShiftDown && go == yInputField.gameObject))
						  EventSystem.current.SetSelectedGameObject(widthInputField.gameObject);
					  else if ((isShiftDown && go == nameInputField.gameObject) || (!isShiftDown && go == widthInputField.gameObject))
						  EventSystem.current.SetSelectedGameObject(heightInputField.gameObject);
					  else if ((isShiftDown && go == xInputField.gameObject) || (!isShiftDown && go == heightInputField.gameObject))
						  EventSystem.current.SetSelectedGameObject(nameInputField.gameObject);
				  });
		Subject<object[]> updateDisplayObjectSubject = MessageBroker.GetSubject(MessageBroker.UpdateSelectDisplayObject);
		updateDisplayObjectSubject.SampleFrame(1)
								  .Subscribe(_ =>
								  {
									  UpdateState(GlobalData.CurrentSelectDisplayObjectDic.Count != 1
													  ? null
													  : GlobalData.CurrentSelectDisplayObjectDic.First().Value);
								  });
		Subject<object[]> updateDisplayObjectPosSubject =
			MessageBroker.GetSubject(MessageBroker.UpdateDisplayObjectPos);
		updateDisplayObjectPosSubject.SampleFrame(1)
									 .Subscribe(_ => UpdateState(_displayObject));
	}

	private static float ParseFloat(string txt)
	{
		float result;
		var bSucceed = float.TryParse(txt, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out result);
		return bSucceed ? result : GlobalData.MinFloat;
	}

	private void UpdateState(Transform displayObject)
	{
		// if (displayObject == _displayObject) return;
		_displayObject = displayObject;

		if (_displayObject == null)
		{
			if (_disposable != null)
			{
				_disposable.Dispose();
				_disposable = null;
			}

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

		// _disposable = _displayObject.GetComponent<RectTransform>()
		// 							.ObserveEveryValueChanged(rect => rect.anchoredPosition)
		// 							.Sample(TimeSpan.FromSeconds(1))
		// 							.Subscribe(anchoredPosition =>
		// 							{
		// 								var pos = Element.ConvertTo(anchoredPosition);
		// 								xInputField.text = $"{pos.x:F1}";
		// 								yInputField.text = $"{pos.y:F1}";
		// 							});
	}

	private static void ChangeXBehavior(string currentModule, string elementName, float x, bool isAdd = false, bool isModify = true)
	{
		if (string.IsNullOrWhiteSpace(currentModule) || !GlobalData.CurrentModule.Equals(currentModule))
			return;
		Transform displayObject = GlobalData.CurrentDisplayObjectDic[elementName];
		if (!displayObject) return;
		var rect = displayObject.GetComponent<RectTransform>();
		var pos = rect.anchoredPosition;
		if (isAdd) pos.x += x;
		else pos.x = Element.InvConvertX(x);
		rect.anchoredPosition = pos;
		Element element = GlobalData.GetElement(elementName);
		if (element == null) return;
		if (isAdd) element.X += x;
		else element.X = x;
		MessageBroker.Send(MessageBroker.UpdateSelectDisplayObject);
		GlobalData.ModifyDic += isModify ? 1 : -1;
	}

	private static void ChangeYBehavior(string currentModule, string elementName, float y, bool isAdd = false, bool isModify = true)
	{
		if (string.IsNullOrWhiteSpace(currentModule) || !GlobalData.CurrentModule.Equals(currentModule))
			return;
		Transform displayObject = GlobalData.CurrentDisplayObjectDic[elementName];
		if (!displayObject) return;
		var rect = displayObject.GetComponent<RectTransform>();
		var pos = rect.anchoredPosition;
		if (isAdd) pos.y += y;
		else pos.y = Element.InvConvertY(y);
		rect.anchoredPosition = pos;
		Element element = GlobalData.GetElement(elementName);
		if (element == null) return;
		if (isAdd) element.Y += y;
		else element.Y = y;
		MessageBroker.Send(MessageBroker.UpdateSelectDisplayObject);
		GlobalData.ModifyDic += isModify ? 1 : -1;
	}

	private static void ChangeWidthBehavior(string currentModule, string elementName, float width, bool isAdd = false, bool isModify = true)
	{
		if (string.IsNullOrWhiteSpace(currentModule) || !GlobalData.CurrentModule.Equals(currentModule))
			return;
		Transform displayObject = GlobalData.CurrentDisplayObjectDic[elementName];
		if (!displayObject) return;
		var rect = displayObject.GetComponent<RectTransform>();
		var size = rect.sizeDelta;
		size.x = isAdd ? Math.Max(size.x + width, 0) : Math.Max(width, 0);
		rect.sizeDelta = size;
		Element element = GlobalData.GetElement(elementName);
		if (element == null) return;
		if (isAdd) element.Width += width;
		else element.Width = width;
		MessageBroker.Send(MessageBroker.UpdateSelectDisplayObject);
		GlobalData.ModifyDic += isModify ? 1 : -1;
	}

	private static void ChangeHeightBehavior(string currentModule, string elementName, float height, bool isAdd = false, bool isModify = true)
	{
		if (string.IsNullOrWhiteSpace(currentModule) || !GlobalData.CurrentModule.Equals(currentModule))
			return;
		Transform displayObject = GlobalData.CurrentDisplayObjectDic[elementName];
		if (!displayObject) return;
		var rect = displayObject.GetComponent<RectTransform>();
		var size = rect.sizeDelta;
		size.y = isAdd ? Math.Max(size.y + height, 0) : Math.Max(height, 0);
		rect.sizeDelta = size;
		Element element = GlobalData.GetElement(elementName);
		if (element == null) return;
		if (isAdd) element.Height += height;
		else element.Height = height;
		MessageBroker.Send(MessageBroker.UpdateSelectDisplayObject);
		GlobalData.ModifyDic += isModify ? 1 : -1;
	}

	private void ChangeNameBehavior(string currentModule, string originName, string newName, bool isModify)
	{
		if (string.IsNullOrWhiteSpace(currentModule) || !GlobalData.CurrentModule.Equals(currentModule))
			return;
		Transform displayObject = GlobalData.CurrentDisplayObjectDic[originName];
		if (displayObject == null) return;
		displayObject.name = newName;
		GlobalData.CurrentDisplayObjectDic.Add(newName, displayObject);
		GlobalData.CurrentDisplayObjectDic.Remove(originName);
		// HierarchyManager.UpdateDisplayObjectName(originName, newName);
		Element element = GlobalData.GetElement(originName);
		if (element != null) element.Name = newName;
		if (GlobalData.CurrentSelectDisplayObjectDic.ContainsKey(originName))
		{
			if (GlobalData.CurrentSelectDisplayObjectDic.Count == 1)
				nameInputField.text = newName;
			GlobalData.CurrentSelectDisplayObjectDic.Remove(originName);
			GlobalData.CurrentSelectDisplayObjectDic.Add(newName, displayObject);
			MessageBroker.Send(MessageBroker.UpdateSelectDisplayObject);
		}
		GlobalData.ModifyDic += isModify ? 1 : -1;
	}
}