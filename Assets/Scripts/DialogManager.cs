using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum DialogType {
	Info,
	Warn,
	Error,
	Question,
	GetValue
}

public class DialogManager : MonoBehaviour {
	public Sprite infoIcon;
	public Sprite warnIcon;
	public Sprite errorIcon;
	public Sprite questionIcon;

	public Image dialogTypeIcon;
	public Text dialogMessage;
	public GameObject messageScrollView;
	public Text valueInfo;
	public InputField valueInputFiled;
	private UnityAction<string> _onGetValue;

	public Button leftButton;
	public Button rightButton;

	private KeyCode _leftKeyCode = KeyCode.None;
	private KeyCode _rightKeyCode = KeyCode.None;

	private static readonly List<Transform> DialogPool = new List<Transform>();

	private static Transform GetDialog() {
		int count = DialogPool.Count;
		if(count == 0) return Instantiate(GlobalData.DialogPrefab.transform, GlobalData.RootCanvas.transform);
		Transform result = DialogPool[count - 1];
		DialogPool.RemoveAt(count - 1);
		result.gameObject.SetActive(true);
		return result;
	}

	private static void RecycleDialog(Transform dialog) {
		if(! dialog) return;
		dialog.gameObject.SetActive(false);
		DialogManager dialogManager = dialog.GetComponent<DialogManager>();
		dialogManager._onGetValue = null;
		dialogManager._leftKeyCode = KeyCode.None;
		dialogManager._rightKeyCode = KeyCode.None;
		DialogPool.Add(dialog);
	}

	public void CloseDialog() {
		RecycleDialog(transform);
	}

	private void Update() {
		if(! Input.anyKey) return;
		if(_leftKeyCode != KeyCode.None && Input.GetKeyDown(_leftKeyCode)) leftButton.onClick.Invoke();
		if(_rightKeyCode != KeyCode.None && Input.GetKeyDown(_rightKeyCode)) rightButton.onClick.Invoke();
	}

	public static DialogManager ShowDialog() {
		Transform dialog = GetDialog();
		EventSystem.current.SetSelectedGameObject(dialog.gameObject);
		return dialog.GetComponent<DialogManager>();
	}

	public static DialogManager ShowInfo(string message, KeyCode rightKeyCode = KeyCode.Return, int dialogWidth = 0, int dialogHeight = 0) {
		DialogManager dialogManager = ShowDialog();
		dialogManager.SetSize(dialogWidth, dialogHeight)
					 .SetDialogType()
					 .SetDialogMessage(message)
					 .SetLeftButtonState(false, "", null, KeyCode.Escape)
					 .SetRightButtonState(true, "确定", null, rightKeyCode);
		return dialogManager;
	}

	public static DialogManager ShowWarn(string message, KeyCode rightKeyCode = KeyCode.Return, int dialogWidth = 0, int dialogHeight = 0) {
		DialogManager dialogManager = ShowDialog();
		dialogManager.SetSize(dialogWidth, dialogHeight)
					 .SetDialogType(DialogType.Warn)
					 .SetDialogMessage(message)
					 .SetLeftButtonState(false, "", null, KeyCode.Escape)
					 .SetRightButtonState(true, "确定", null, rightKeyCode);
		return dialogManager;
	}

	public static DialogManager ShowError(string message, KeyCode rightKeyCode = KeyCode.Return, int dialogWidth = 700, int dialogHeight = 400) {
		DialogManager dialogManager = ShowDialog();
		dialogManager.SetSize(dialogWidth, dialogHeight)
					 .SetDialogType(DialogType.Error)
					 .SetDialogMessage(message)
					 .SetLeftButtonState(false, "", null, KeyCode.Escape)
					 .SetRightButtonState(true, "确定", null, rightKeyCode);
		return dialogManager;
	}

	public static DialogManager ShowQuestion(string      message,
											 UnityAction onLeftButtonClick,
											 UnityAction onRightButtonClick,
											 string      leftButtonTxt  = "确定",
											 string      rightButtonTxt = "取消",
											 KeyCode     leftKeyCode    = KeyCode.Return,
											 KeyCode     rightKeyCode   = KeyCode.Escape,
											 int         dialogWidth    = 0,
											 int         dialogHeight   = 0) {
		DialogManager dialogManager = ShowDialog();
		dialogManager.SetSize(dialogWidth, dialogHeight)
					 .SetDialogType(DialogType.Question)
					 .SetDialogMessage(message)
					 .SetLeftButtonState(true, leftButtonTxt, onLeftButtonClick, leftKeyCode)
					 .SetRightButtonState(true, rightButtonTxt, onRightButtonClick, rightKeyCode);
		return dialogManager;
	}

	public static DialogManager ShowGetValue(string              valueInfo,
											 string              placeholderText,
											 UnityAction<string> onGetValue,
											 UnityAction         onLeftButtonClick  = null,
											 UnityAction         onRightButtonClick = null,
											 string              leftButtonTxt      = "确定",
											 string              rightButtonTxt     = "取消",
											 KeyCode             leftKeyCode        = KeyCode.Return,
											 KeyCode             rightKeyCode       = KeyCode.Escape,
											 int                 dialogWidth        = 360,
											 int                 dialogHeight       = 0) {
		DialogManager dialogManager = ShowDialog();
		dialogManager.SetSize(dialogWidth, dialogHeight)
					 .SetDialogType(DialogType.GetValue)
					 .SetOnGetValue(onGetValue)
					 .SetValueInfoText(valueInfo)
					 .SetValuePlaceholder(placeholderText)
					 .SetLeftButtonState(true, leftButtonTxt, onLeftButtonClick, leftKeyCode)
					 .SetRightButtonState(true, rightButtonTxt, onRightButtonClick, rightKeyCode);
		EventSystem.current.SetSelectedGameObject(dialogManager.valueInputFiled.gameObject);
		return dialogManager;
	}

	public DialogManager SetSize(int dialogWidth, int dialogHeight) {
		dialogWidth = Math.Max(dialogWidth, 280);
		dialogHeight = Math.Max(dialogHeight, 160);
		transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(dialogWidth, dialogHeight);
		return this;
	}

	public DialogManager SetDialogType(DialogType dialogDialogType = DialogType.Info) {
		Sprite sprite = null;
		int mainType;
		switch(dialogDialogType) {
			case DialogType.Info:
				sprite = infoIcon;
				mainType = 1;
				break;
			case DialogType.Warn:
				sprite = warnIcon;
				mainType = 1;
				break;
			case DialogType.Error:
				sprite = errorIcon;
				mainType = 1;
				break;
			case DialogType.Question:
				sprite = questionIcon;
				mainType = 1;
				break;
			case DialogType.GetValue:
				mainType = 2;
				break;
			default:
				return this;
		}

		dialogTypeIcon.gameObject.SetActive(mainType == 1);
		messageScrollView.SetActive(mainType == 1);
		valueInfo.gameObject.SetActive(mainType == 2);
		valueInputFiled.gameObject.SetActive(mainType == 2);
		if(mainType == 1) dialogTypeIcon.sprite = sprite;
		return this;
	}

	public DialogManager SetDialogMessage(string txt) {
		if(string.IsNullOrEmpty(txt)) return this;
		dialogMessage.text = txt;
		return this;
	}

	public DialogManager SetOnGetValue(UnityAction<string> onGetValue) {
		_onGetValue = onGetValue;
		return this;
	}

	public DialogManager SetLeftButtonState(bool bShow, string txt = "Yes", UnityAction onLeftButtonClick = null, KeyCode leftKeyCode = KeyCode.None) {
		leftButton.gameObject.SetActive(bShow);
		leftButton.GetComponentInChildren<Text>().text = txt;
		leftButton.onClick.RemoveAllListeners();
		if(onLeftButtonClick != null) leftButton.onClick.AddListener(onLeftButtonClick);
		if(_onGetValue != null) leftButton.onClick.AddListener(() => _onGetValue(valueInputFiled.text));
		leftButton.onClick.AddListener(CloseDialog);
		_leftKeyCode = leftKeyCode;
		return this;
	}

	public DialogManager SetRightButtonState(bool bShow, string txt = "Yes", UnityAction onRightButtonClick = null, KeyCode rightKeyCode = KeyCode.None) {
		rightButton.gameObject.SetActive(bShow);
		rightButton.GetComponentInChildren<Text>().text = txt;
		rightButton.onClick.RemoveAllListeners();
		if(onRightButtonClick != null) rightButton.onClick.AddListener(onRightButtonClick);
		rightButton.onClick.AddListener(CloseDialog);
		_rightKeyCode = rightKeyCode;
		return this;
	}

	public DialogManager SetMessageFontSize(int fontSize) {
		GetComponent<Text>().fontSize = fontSize;
		return this;
	}

	public DialogManager SetValueInfoText(string txt) {
		if(string.IsNullOrEmpty(txt)) return this;
		valueInfo.text = txt;
		return this;
	}

	public DialogManager SetValuePlaceholder(string txt) {
		if(string.IsNullOrEmpty(txt)) return this;
		valueInputFiled.placeholder.GetComponent<Text>().text = txt;
		valueInputFiled.text = string.Empty;
		return this;
	}
}
