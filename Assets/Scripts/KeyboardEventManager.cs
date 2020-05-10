using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyboardEventManager : MonoBehaviour {
	public ContainerManager containerManager;
	public FunctionButtonHandler functionButtonHandler;
	public ScrollRect containerScrollRect;
	public RectTransform containerRect;
	public float containerKeyMoveSensitivity;
	public Slider scaleSlider;

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

	private void Update() {
		if(Input.anyKeyDown) UpdateShortcut();
		UpdateContainer();
	}

	private void UpdateShortcut() {
		bool isFocusOnInputText = Utils.IsFocusOnInputText();
		bool isControlDown = GetControl();
		bool isShiftDown = GetShift();
		bool isAltDown = GetAlt();
		if(isControlDown) {
			if(Input.GetKeyDown(KeyCode.M))
				functionButtonHandler.OnCreateModuleButtonClick();
			else if(Input.GetKeyDown(KeyCode.N)) {
				Vector2 pos = Utils.GetRealPositionInContainer(Input.mousePosition);
				DisplayObjectUtil.AddDisplayObject(null, pos, GlobalData.DefaultSize, null, true);
			} else if(Input.GetKeyDown(KeyCode.Backspace))
				functionButtonHandler.OnRemoveButtonClick();
			else if(Input.GetKeyDown(KeyCode.UpArrow))
				functionButtonHandler.OnUpButtonClick();
			else if(Input.GetKeyDown(KeyCode.DownArrow))
				functionButtonHandler.OnDownButtonClick();
			else if(Input.GetKeyDown(KeyCode.P))
				functionButtonHandler.OnCopyButtonClick();
			else if(Input.GetKeyDown(KeyCode.I))
				functionButtonHandler.OnImportButtonClick();
			else if(Input.GetKeyDown(KeyCode.E))
				functionButtonHandler.OnExportButtonClick();
			else if(Input.GetKeyDown(KeyCode.H))
				functionButtonHandler.OnHelpButtonClick();
			else if(Input.GetKeyDown(KeyCode.T) && GlobalData.FrameToggle)
				functionButtonHandler.OnToggleTrigger(! GlobalData.FrameToggle.isOn);
		}

		if(Input.GetKeyDown(KeyCode.Delete) && ! isFocusOnInputText) functionButtonHandler.OnRemoveButtonClick();

		if(Input.GetKeyDown(KeyCode.Escape) && GlobalData.CurrentSelectDisplayObjectDic.Count != 0 && ! isFocusOnInputText) {
			List<string> removeElements = GlobalData.CurrentSelectDisplayObjectDic.KeyList();
			if(removeElements != null)
				HistoryManager.Do(BehaviorFactory.GetUpdateSelectDisplayObjectBehavior(GlobalData.CurrentModule, null, removeElements));
		}

		if(isControlDown && ! isFocusOnInputText) {
			if(Input.GetKeyDown(KeyCode.C))
				DisplayObjectUtil.CopySelectDisplayObjects();
			else if(Input.GetKeyDown(KeyCode.V)) containerManager.PasteDisplayObjects();
		}

		if(isControlDown) {
			if(Input.GetKeyDown(KeyCode.Z))
				HistoryManager.Undo();
			else if(Input.GetKeyDown(KeyCode.Y)) HistoryManager.Do();
		}

		if(isControlDown && Input.GetKeyDown(KeyCode.S)) {
			if(string.IsNullOrWhiteSpace(GlobalData.CurrentFilePath))
				functionButtonHandler.OnExportButtonClick();
			else
				ModuleUtil.ExportModules(GlobalData.CurrentFilePath, true);
		}

		if(isControlDown && isShiftDown && isAltDown && Input.GetKeyDown(KeyCode.D)) {
			Debugger.ShowDebugging = ! Debugger.ShowDebugging;
		}

		if(isControlDown && isShiftDown && isAltDown && Input.GetKeyDown(KeyCode.F)) Screen.fullScreen = ! Screen.fullScreen;

		if(Input.GetKeyDown(KeyCode.Q)) Debug.Log($"pos: {Utils.GetRealPosition(Input.mousePosition)}");
	}

	private void UpdateContainer() {
		bool isControlDown = GetControl();
		bool isShiftDown = GetShift();
		float mouseScrollValue = Input.GetAxis("Mouse ScrollWheel");
		if(isControlDown && Math.Abs(mouseScrollValue) > 0.001f) {
			Vector2 prevPos = Utils.GetAnchoredPositionInContainer(Input.mousePosition);
			scaleSlider.value += mouseScrollValue * 10;
			Vector2 currPos = Utils.GetAnchoredPositionInContainer(Input.mousePosition);
			Vector2 offset = currPos - prevPos;
			Vector3 localScale = containerRect.localScale;
			Debug.Log($"prev: {prevPos}, curr: {currPos}, offset: {offset}, scale: {localScale.x}");
			containerRect.anchoredPosition += offset * localScale.x;
		}

		if(isControlDown || Input.GetMouseButton(0)) {
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
			containerScrollRect.horizontal = isShiftDown;
			containerScrollRect.vertical = ! isShiftDown;
			containerScrollRect.scrollSensitivity = Math.Abs(containerScrollRect.scrollSensitivity) * (isShiftDown ? -1 : 1);
		}

		if(isControlDown || ! Input.anyKey) return;
		if(GetAlt()) {
			Transform displayObject = GlobalData.CurrentSelectDisplayObjectDic.OnlyValue();
			if(! displayObject) return;
			if(Input.GetKeyDown(KeyCode.UpArrow)) DisplayObjectUtil.SelectDisplayObjectByOffset(displayObject, -1, isShiftDown);
			else if(Input.GetKeyDown(KeyCode.DownArrow)) DisplayObjectUtil.SelectDisplayObjectByOffset(displayObject, 1, isShiftDown);
			return;
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
		containerRect.anchoredPosition += delta;
	}
}
