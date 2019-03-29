using System;
using System.Globalization;
using UniRx;
using UnityEngine;
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
        NameInputField.ObserveEveryValueChanged(element => element.isFocused)
            .Where(isFocused => !isFocused && !string.IsNullOrEmpty(NameInputField.text))
            .Subscribe(_ => {
                if (!_displayObject) return;
                _displayObject.name = NameInputField.text;
            });
        XInputField.ObserveEveryValueChanged(element => element.isFocused)
            .Where(isFocused => !isFocused && !string.IsNullOrEmpty(XInputField.text))
            .Subscribe(_ =>
            {
                var x = ParseFloat(XInputField.text);
                if (x <= GlobalData.MinFloat) return;
                if (!_displayObject) return;
                var rect = _displayObject.GetComponent<RectTransform>();
                var pos = rect.anchoredPosition;
                pos.x = DisplayObject.InvConvertX(x);
                rect.anchoredPosition = pos;
            });
        YInputField.ObserveEveryValueChanged(element => element.isFocused)
            .Where(isFocused => !isFocused && !string.IsNullOrEmpty(YInputField.text))
            .Subscribe(_ => {
                var y = ParseFloat(YInputField.text);
                if (y <= GlobalData.MinFloat) return;
                if (!_displayObject) return;
                var rect = _displayObject.GetComponent<RectTransform>();
                var pos = rect.anchoredPosition;
                pos.y = DisplayObject.InvConvertY(y);
                rect.anchoredPosition = pos;
            });
        WidthInputField.ObserveEveryValueChanged(element => element.isFocused)
            .Where(isFocused => !isFocused && !string.IsNullOrEmpty(WidthInputField.text))
            .Subscribe(_ => {
                var width = ParseFloat(WidthInputField.text);
                if (width < 0f) return;
                if (!_displayObject) return;
                var rect = _displayObject.GetComponent<RectTransform>();
                var size = rect.sizeDelta;
                size.x = width;
                rect.sizeDelta = size;
            });
        HeightInputField.ObserveEveryValueChanged(element => element.isFocused)
            .Where(isFocused => !isFocused && !string.IsNullOrEmpty(HeightInputField.text))
            .Subscribe(_ => {
                var height = ParseFloat(HeightInputField.text);
                if (height < 0f) return;
                if (!_displayObject) return;
                var rect = _displayObject.GetComponent<RectTransform>();
                var size = rect.sizeDelta;
                size.y = height;
                rect.sizeDelta = size;
            });
    }

    private static float ParseFloat(string txt)
    {
        float result;
        var bSucceed = float.TryParse(txt, NumberStyles.Float, NumberFormatInfo.CurrentInfo, out result);
        if (!bSucceed) return GlobalData.MinFloat;
        return result;
    }

    private void Update()
    {
        var length = GlobalData.CurrentSelectDisplayObjects.Count;
        if (length != 1)
            UpdateState(null);
        else
            foreach (var pair in GlobalData.CurrentSelectDisplayObjects)
                UpdateState(pair.Value);
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