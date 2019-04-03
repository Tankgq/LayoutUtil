﻿using System;
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

    public InputField NameInputField;
    public InputField XInputField;
    public InputField YInputField;
    public InputField WidthInputField;
    public InputField HeightInputField;
    
    private void Start()
    {
        EventSystem.current.SetSelectedGameObject(NameInputField.gameObject);
        NameInputField.ObserveEveryValueChanged(element => element.isFocused)
            .Where(isFocus => _displayObject && !isFocus && !string.IsNullOrEmpty(NameInputField.text))
            .Subscribe(_ => {
                _displayObject.name = NameInputField.text;
            });
        XInputField.ObserveEveryValueChanged(element => element.isFocused)
            .Where(isFocused => _displayObject && !isFocused && !string.IsNullOrEmpty(XInputField.text))
            .Select(_ => ParseFloat(XInputField.text))
            .Where(x => x > GlobalData.MinFloat)
            .Subscribe(x =>
            {
                var rect = _displayObject.GetComponent<RectTransform>();
                var pos = rect.anchoredPosition;
                pos.x = DisplayObject.InvConvertX(x);
                rect.anchoredPosition = pos;
            });
        YInputField.ObserveEveryValueChanged(element => element.isFocused)
            .Where(isFocused => _displayObject && !isFocused && !string.IsNullOrEmpty(YInputField.text))
            .Select(_ => ParseFloat(YInputField.text))
            .Where(y => y > GlobalData.MinFloat)
            .Subscribe(y =>
            {
                var rect = _displayObject.GetComponent<RectTransform>();
                var pos = rect.anchoredPosition;
                pos.y = DisplayObject.InvConvertY(y);
                rect.anchoredPosition = pos;
            });
        WidthInputField.ObserveEveryValueChanged(element => element.isFocused)
            .Where(isFocused => _displayObject && !isFocused && !string.IsNullOrEmpty(WidthInputField.text))
            .Select(_ => ParseFloat(WidthInputField.text))
            .Where(width => width >= 0f)
            .Subscribe(width =>
            {
                var rect = _displayObject.GetComponent<RectTransform>();
                var size = rect.sizeDelta;
                size.x = width;
                rect.sizeDelta = size;
            });
        HeightInputField.ObserveEveryValueChanged(element => element.isFocused)
            .Where(isFocused => _displayObject && !isFocused && !string.IsNullOrEmpty(HeightInputField.text))
            .Select(_ => ParseFloat(HeightInputField.text))
            .Where(height => height >= 0f)
            .Subscribe(height =>
            {
                var rect = _displayObject.GetComponent<RectTransform>();
                var size = rect.sizeDelta;
                size.y = height;
                rect.sizeDelta = size;
            });
        Observable.EveryUpdate()
            .Where(_ => EventSystem.current.currentSelectedGameObject != null && Input.GetKeyDown(KeyCode.Tab))
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
            .Subscribe(count => {
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
        XInputField.text = $"{display.X}";
        YInputField.text = $"{display.Y}";
        WidthInputField.text = $"{display.Width}";
        HeightInputField.text = $"{display.Height}";

        _disposable = _displayObject.GetComponent<RectTransform>()
            .ObserveEveryValueChanged(rect => rect.anchoredPosition)
            .Sample(TimeSpan.FromSeconds(1))
            .Subscribe(anchoredPosition =>
            {
                var pos = DisplayObject.ConvertTo(anchoredPosition);
                XInputField.text = $"{pos.x}";
                YInputField.text = $"{pos.y}";
            });
    }
}