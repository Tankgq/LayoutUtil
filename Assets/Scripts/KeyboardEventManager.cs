﻿using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardEventManager : MonoBehaviour
{
    public ContainerManager ContainerManager;
    public Slider ScaleSlider;

    public static bool IsShiftDown()
    {
        return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }

    public static bool IsControlDown() {
        return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
    }

    public static bool IsEnterDown()
    {
        return Input.GetKey(KeyCode.KeypadEnter);
    }

    private void Start()
    {
        Observable.EveryUpdate()
            .Where(_ => Input.GetKeyDown(KeyCode.Backspace) && GlobalData.CurrentSelectDisplayObjects.Count != 0)
            .Subscribe(_ => ContainerManager.RemoveSelectedDisplayObject());
        Observable.EveryUpdate()
            .Where(_ => Input.GetKeyDown(KeyCode.Escape) && GlobalData.CurrentSelectDisplayObjects.Count != 0)
            .Subscribe(_ => GlobalData.CurrentSelectDisplayObjects.Clear());
        Observable.EveryUpdate()
            .Where(_ => IsControlDown() && Math.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0.001f)
            .Subscribe(_ => ScaleSlider.value += Input.GetAxis("Mouse ScrollWheel") * 10);
    }
}