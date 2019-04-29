using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts
{
	public class InspectorManager : MonoBehaviour
	{
		private Transform _displayObject;
		private IDisposable _disposable;

		public InputField NameInputField;
		public InputField XInputField;
		public InputField YInputField;
		public InputField WidthInputField;
		public InputField HeightInputField;

		private void Start()
		{
			EventSystem.current.SetSelectedGameObject(NameInputField.gameObject);
			NameInputField.ObserveEveryValueChanged(element => element.isFocused)
				.Where(isFocus => !string.IsNullOrWhiteSpace(GlobalData.CurrentModule) && _displayObject && !isFocus && !string.IsNullOrEmpty(NameInputField.text))
				.Subscribe(_ =>
				{
					string displayObjectKey = NameInputField.text;
					string originName = GlobalData.CurrentSelectDisplayObjectDic.First().Value.name;
					if (displayObjectKey.Equals(originName)) return;
					if (GlobalData.CurrentDisplayObjectDic.ContainsKey(displayObjectKey))
					{
						DialogManager.ShowError("该名称已存在", 0, 0);
						NameInputField.text = originName;
						return;
					}
					string originDisplayObjectKey = GlobalData.CurrentSelectDisplayObjectDic.First().Key;
					Transform displayObject = GlobalData.CurrentDisplayObjectDic[originDisplayObjectKey];
					displayObject.name = NameInputField.text;
					GlobalData.CurrentDisplayObjectDic.Add(displayObjectKey, displayObject);
					GlobalData.CurrentDisplayObjectDic.Remove(originDisplayObjectKey);
					HierarchyManager.UpdateDisplayObjectName(originName, NameInputField.text);
					_displayObject.name = NameInputField.text;
				});
			XInputField.ObserveEveryValueChanged(element => element.isFocused)
				.Where(isFocused => !isFocused && !string.IsNullOrEmpty(XInputField.text))
				.Select(_ => ParseFloat(XInputField.text))
				.Where(x => x > GlobalData.MinFloat)
				.Subscribe(x =>
				{
					if (_displayObject)
					{
						updateX(_displayObject, x);
						return;
					}
					if (GlobalData.CurrentSelectDisplayObjectDic.Count > 1)
						foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic)
							updateX(pair.Value, x, true);
					XInputField.text = "0";
				});
			YInputField.ObserveEveryValueChanged(element => element.isFocused)
				.Where(isFocused => !isFocused && !string.IsNullOrEmpty(YInputField.text))
				.Select(_ => ParseFloat(YInputField.text))
				.Where(y => y > GlobalData.MinFloat)
				.Subscribe(y =>
				{
					if (_displayObject)
					{
						updateY(_displayObject, y);
						return;
					}
					if (GlobalData.CurrentSelectDisplayObjectDic.Count > 1)
						foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic)
							updateY(pair.Value, y, true);
					YInputField.text = "0";
				});
			WidthInputField.ObserveEveryValueChanged(element => element.isFocused)
				.Where(isFocused => !isFocused && !string.IsNullOrEmpty(WidthInputField.text))
				.Select(_ => ParseFloat(WidthInputField.text))
				.Where(width => width > GlobalData.MinFloat)
				.Subscribe(width =>
				{
					if (_displayObject)
					{
						updateWidth(_displayObject, width);
						return;
					}
					if (GlobalData.CurrentSelectDisplayObjectDic.Count > 1)
						foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic)
							updateWidth(pair.Value, width, true);
					WidthInputField.text = "0";
				});
			HeightInputField.ObserveEveryValueChanged(element => element.isFocused)
				.Where(isFocused => !isFocused && !string.IsNullOrEmpty(HeightInputField.text))
				.Select(_ => ParseFloat(HeightInputField.text))
				.Where(height => height > GlobalData.MinFloat)
				.Subscribe(height =>
				{
					if (_displayObject)
					{
						updateHeight(_displayObject, height);
						return;
					}
					if (GlobalData.CurrentSelectDisplayObjectDic.Count > 1)
						foreach (var pair in GlobalData.CurrentSelectDisplayObjectDic)
							updateHeight(pair.Value, height, true);
					HeightInputField.text = "0";
				});
			Observable.EveryUpdate()
				.Where(_ => EventSystem.current.currentSelectedGameObject != null && (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Return)))
				.Subscribe(_ =>
				{
					GameObject go = EventSystem.current.currentSelectedGameObject;
					if (go == NameInputField.gameObject)
						EventSystem.current.SetSelectedGameObject(XInputField.gameObject);
					else if (go == XInputField.gameObject)
						EventSystem.current.SetSelectedGameObject(YInputField.gameObject);
					else if (go == YInputField.gameObject)
						EventSystem.current.SetSelectedGameObject(WidthInputField.gameObject);
					else if (go == WidthInputField.gameObject)
						EventSystem.current.SetSelectedGameObject(HeightInputField.gameObject);
					else if (go == HeightInputField.gameObject)
						EventSystem.current.SetSelectedGameObject(NameInputField.gameObject);
				});
			GlobalData.CurrentSelectDisplayObjectDic.ObserveEveryValueChanged(dic => dic.Count)
				.Subscribe(count =>
				{
					if (count != 1) UpdateState(null);
					else UpdateState(GlobalData.CurrentSelectDisplayObjectDic.First().Value);
				});
		}

		private static float ParseFloat(string txt)
		{
			float result;
			var bSucceed = float.TryParse(txt, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out result);
			return bSucceed ? result : GlobalData.MinFloat;
		}

		private void UpdateState(Transform displayObject)
		{
			if (displayObject == _displayObject) return;
			_displayObject = displayObject;

			if (_displayObject == null)
			{
				if (_disposable != null)
				{
					_disposable.Dispose();
					_disposable = null;
				}

				NameInputField.text = "null";
				XInputField.text = "0";
				YInputField.text = "0";
				WidthInputField.text = "0";
				HeightInputField.text = "0";
				return;
			}

			var display = DisplayObject.ConvertTo(_displayObject);
			NameInputField.text = display.Name;
			XInputField.text = $"{display.X:F1}";
			YInputField.text = $"{display.Y:F1}";
			WidthInputField.text = $"{display.Width:F1}";
			HeightInputField.text = $"{display.Height:F1}";

			_disposable = _displayObject.GetComponent<RectTransform>()
				.ObserveEveryValueChanged(rect => rect.anchoredPosition)
				.Sample(TimeSpan.FromSeconds(1))
				.Subscribe(anchoredPosition =>
				{
					var pos = DisplayObject.ConvertTo(anchoredPosition);
					XInputField.text = $"{pos.x:F1}";
					YInputField.text = $"{pos.y:F1}";
				});
		}

		private void updateX(Transform displayObject, float x, bool isAdd = false)
		{
			var rect = displayObject.GetComponent<RectTransform>();
			var pos = rect.anchoredPosition;
			if (isAdd) pos.x += x;
			else pos.x = DisplayObject.InvConvertX(x);
			rect.anchoredPosition = pos;
			DisplayObject displayObjectData = GlobalData.GetDisplayObjectData(displayObject.name);
			if (displayObjectData == null) return;
			if (isAdd) displayObjectData.X += x;
			else displayObjectData.X = x;
		}

		private void updateY(Transform displayObject, float y, bool isAdd = false)
		{
			var rect = displayObject.GetComponent<RectTransform>();
			var pos = rect.anchoredPosition;
			if (isAdd) pos.y -= y;
			else pos.y = DisplayObject.InvConvertY(y);
			rect.anchoredPosition = pos;
			DisplayObject displayObjectData = GlobalData.GetDisplayObjectData(displayObject.name);
			if (displayObjectData == null) return;
			if (isAdd) displayObjectData.Y += y;
			else displayObjectData.Y = y;
		}

		private void updateWidth(Transform displayObject, float width, bool isAdd = false)
		{
			var rect = displayObject.GetComponent<RectTransform>();
			var size = rect.sizeDelta;
			if (isAdd) size.x = Math.Max(size.x + width, 0);
			else size.x = Math.Max(width, 0); ;
			rect.sizeDelta = size;
			DisplayObject displayObjectData = GlobalData.GetDisplayObjectData(displayObject.name);
			if (displayObjectData == null) return;
			if (isAdd) displayObjectData.Width += width;
			else displayObjectData.Width = width;
		}

		private void updateHeight(Transform displayObject, float height, bool isAdd = false)
		{
			var rect = displayObject.GetComponent<RectTransform>();
			var size = rect.sizeDelta;
			if (isAdd) size.y = Math.Max(size.y + height, 0);
			else size.y = Math.Max(height, 0);
			rect.sizeDelta = size;
			DisplayObject displayObjectData = GlobalData.GetDisplayObjectData(displayObject.name);
			if (displayObjectData == null) return;
			if (isAdd) displayObjectData.Height += height;
			else displayObjectData.Height = height;
		}
	}
}