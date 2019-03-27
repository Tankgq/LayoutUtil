using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class InspectorManager : MonoBehaviour
{
    public Transform nameElement;
    public Transform xElement;
    public Transform yElement;
    public Transform widthElement;
    public Transform heightElement;

    private InputField _nameIf;
    private InputField _xIf;
    private InputField _yIf;
    private InputField _widthIf;
    private InputField _heightIf;

    private Transform _displayObject;

    private void Start()
    {
        _nameIf = nameElement.GetComponentInChildren<InputField>();
        _xIf = xElement.GetComponentInChildren<InputField>();
        _yIf = yElement.GetComponentInChildren<InputField>();
        _widthIf = widthElement.GetComponentInChildren<InputField>();
        _heightIf = heightElement.GetComponentInChildren<InputField>();

        _nameIf.OnValueChangedAsObservable()
            .Where(txt => ! string.IsNullOrEmpty(txt))
            .Sample(TimeSpan.FromSeconds(1))
            .Subscribe(txt =>
        {
            if (!_displayObject) return;
            _displayObject.name = txt;
        });
    }

    private void Update()
    {
        int length = GlobalData.curSelectDisplayObjects.Count;
        if (length != 1)
        {
            UpdateState(null);
        }
        else
        {
            foreach(var pair in GlobalData.curSelectDisplayObjects)
                UpdateState(pair.Value);
        }
    }
    
    private void UpdateState(Transform displayObject)
    {
        if(displayObject == _displayObject) return;
        _displayObject = displayObject;

        if (_displayObject == null)
        {
            _nameIf.text = "null";
            _xIf.text = "0";
            _yIf.text = "0";
            _widthIf.text = "0";
            _heightIf.text = "0";
            return;
        }

        DisplayObject display = DisplayObjectManager.ConvertToDisplayObject(_displayObject);
        _nameIf.text = display.name;
        _xIf.text = "" + display.x;
        _yIf.text = "" + display.y;
        _widthIf.text = "" + display.x;
        _heightIf.text = "" + display.y;

        _displayObject.ObserveEveryValueChanged(element => element.GetComponent<RectTransform>().anchoredPosition)
            .Sample(TimeSpan.FromSeconds(1))
            .Subscribe(pos => {
                _xIf.text = "" + pos.x;
                _yIf.text = "" + (-pos.y);
            });
    }
}
