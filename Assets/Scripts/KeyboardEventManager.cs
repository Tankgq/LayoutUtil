﻿using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class KeyboardEventManager : MonoBehaviour {
	public ContainerManager containerManager;
	public ScrollRect containerScrollRect;
	public RectTransform containerRect;
	public float containerKeyMoveSensitivity;
	public Slider scaleSlider;
	public DragFileUtil dragFileUtil;

	public static bool GetShift() {
		return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
	}

	public static bool GetShiftDown() {
		return Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift);
	}

	public static bool GetControl() {
		return Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
	}

	public static bool GetControlDown() {
		return Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl);
	}

	public static bool GetAlt() {
		return Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
	}

	public static bool GetAltDown() {
		return Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt);
	}

	private Vector3 _containerOffset = Vector3.zero;

	private void Start() {
		Observable.EveryUpdate()
				  .Where(_ => (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete)) && ! Utils.IsFocusOnInputText())
				  .Subscribe(_ => ContainerManager.RemoveSelectedDisplayObjectOrModules());
		Observable.EveryUpdate()
				  .Where(_ => Input.GetKeyDown(KeyCode.Escape) && GlobalData.CurrentSelectDisplayObjectDic.Count != 0 && ! Utils.IsFocusOnInputText())
				  .Subscribe(_ => GlobalData.CurrentSelectDisplayObjectDic.Clear());
		Observable.EveryUpdate()
				  .Where(_ => GetControl() && Math.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0.001f)
				  .Select(_ => Input.GetAxis("Mouse ScrollWheel"))
				  .Subscribe(scrollValue => {
					   Vector2 prevPos = Utils.GetAnchoredPositionInContainer(Input.mousePosition);
					   scaleSlider.value += scrollValue * 10;
					   Vector2 currPos = Utils.GetAnchoredPositionInContainer(Input.mousePosition);
					   Vector2 offset = currPos - prevPos;
					   Vector3 localScale = containerRect.localScale;
					   Debug.Log($"prev: {prevPos}, curr: {currPos}, offset: {offset}, scale: {localScale.x}");
					   containerRect.anchoredPosition += offset * localScale.x;
				   });
		Observable.EveryUpdate()
				  .Subscribe(_ => {
					   if(GetControl() || Input.GetMouseButton(0)) {
						   containerScrollRect.horizontal = false;
						   containerScrollRect.vertical = false;
					   } else if(Input.GetMouseButton(2)) {
						   containerScrollRect.horizontal = true;
						   containerScrollRect.vertical = true;
						   if(Input.GetMouseButtonDown(2)) {
							   _containerOffset = containerRect.anchoredPosition3D - Input.mousePosition;
						   } else {
							   containerRect.anchoredPosition3D = Input.mousePosition + _containerOffset;
						   }
					   } else {
						   bool isShiftDown = GetShift();
						   containerScrollRect.horizontal = isShiftDown;
						   containerScrollRect.vertical = ! isShiftDown;
						   containerScrollRect.scrollSensitivity = Math.Abs(containerScrollRect.scrollSensitivity) * (isShiftDown ? -1 : 1);
					   }
					   Vector2 delta = Vector2.zero;
					   if(Input.GetKey(KeyCode.UpArrow)) {
						   delta += Vector2.up * containerKeyMoveSensitivity;
					   }
					   if(Input.GetKey(KeyCode.DownArrow)) {
						   delta += Vector2.down * containerKeyMoveSensitivity;
					   }
					   if(Input.GetKey(KeyCode.LeftArrow)) {
						   delta += Vector2.left * containerKeyMoveSensitivity;
					   }
					   if(Input.GetKey(KeyCode.RightArrow)) {
						   delta += Vector2.right * containerKeyMoveSensitivity;
					   }

					   if(Utils.IsFocusOnInputText() || Utils.IsEqual(delta.x, 0) && Utils.IsEqual(delta.y, 0)) return;
					   Debug.Log($"delta: {delta}");
					   containerRect.anchoredPosition = containerRect.anchoredPosition + delta;
				   });
		Observable.EveryUpdate()
				  .Where(_ => Input.GetKeyDown(KeyCode.N) && GetControl())
				  .Subscribe(_ => {
					   Vector2 pos = Utils.GetRealPositionInContainer(Input.mousePosition);
					   DisplayObjectUtil.AddDisplayObject(null, pos, GlobalData.DefaultSize);
				   });
		Observable.EveryUpdate()
				  .Where(_ => Input.GetKeyDown(KeyCode.D) && GetShift() && GetAlt())
				  .Sample(TimeSpan.FromMilliseconds(100))
				  .Subscribe(_ => {
					   Debugger.ShowDebugging = ! Debugger.ShowDebugging;
					   Debug.Log($"Debugger.ShowDebugging: {Debugger.ShowDebugging}");
				   });
		Observable.EveryUpdate()
				  .Where(_ => Input.GetKeyDown(KeyCode.F) && GetShift() && GetAlt())
				  .Subscribe(_ => Screen.fullScreen = ! Screen.fullScreen);
		Observable.EveryUpdate()
				  .Where(_ => Input.GetKeyDown(KeyCode.C) && GetControl() && ! Utils.IsFocusOnInputText())
				  .Subscribe(_ => DisplayObjectUtil.CopySelectDisplayObjects());
		Observable.EveryUpdate()
				  .Where(_ => Input.GetKeyDown(KeyCode.V) && GetControl() && ! Utils.IsFocusOnInputText())
				  .Subscribe(_ => containerManager.PasteDisplayObjects());
		Observable.EveryUpdate()
				  .Where(_ => Input.GetKeyDown(KeyCode.Q))
				  .Subscribe(_ => Debug.Log($"pos: {Utils.GetAnchoredPositionInContainer(Input.mousePosition) + containerRect.anchoredPosition}"));
		Observable.EveryUpdate()
				  .Where(_ => Input.GetKeyDown(KeyCode.Z) && GetControl())
				  .Subscribe(_ => HistoryManager.Undo());
		Observable.EveryUpdate()
				  .Where(_ => Input.GetKeyDown(KeyCode.Y) && GetControl())
				  .Subscribe(_ => HistoryManager.Do());
		Observable.EveryUpdate()
				  .Where(_ => Input.GetKeyDown(KeyCode.S) && GetControl())
				  .Subscribe(_ => ModuleUtil.ExportModules(GlobalData.CurrentFilePath, true));
		Observable.EveryUpdate()
				  .Where(_ => Input.GetKeyDown(KeyCode.M) && GetControl())
				  .Subscribe(_ => ModuleUtil.CreateModule());
	}
}
