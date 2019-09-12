using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
	public const int InfoType = 0;
	public const int WarnType = 1;
	public const int ErrorType = 2;
	public const int QuestionType = 3;
	public const int GetValue = 4;

	public Sprite InfoIcon;
	public Sprite WarnIcon;
	public Sprite ErrorIcon;
	public Sprite QuestionIcon;

	public Image DialogTypeIcon;
	public Text DialogMessage;
	public GameObject MessageScrollView;
	public Text ValueInfo;
	public InputField ValueInputFiled;
	private UnityAction<string> OnGetValue;

	public Button LeftButton;
	public Button RightButton;

	private static readonly List<Transform> DialogPool = new List<Transform>();

	private static Transform GetDialog()
	{
		int count = DialogPool.Count;
		if (count == 0) return Instantiate(GlobalData.DialogPrefab.transform, GlobalData.RootCanvas.transform);
		Transform result = DialogPool[count - 1];
		DialogPool.RemoveAt(count - 1);
		result.gameObject.SetActive(true);
		return result;
	}

	public static void RecycleDialog(Transform dialog)
	{
		if (!dialog) return;
		dialog.gameObject.SetActive(false);
		dialog.GetComponent<DialogManager>().OnGetValue = null;
		DialogPool.Add(dialog);
	}

	public void CloseDialog()
	{
		RecycleDialog(transform);
	}

	public static DialogManager ShowDialog()
	{
		Transform dialog = GetDialog();
		return dialog.GetComponent<DialogManager>();
	}

	public static DialogManager ShowInfo(string message, int dialogWidth = 0, int dialogHeight = 0)
	{
		DialogManager dialogManager = ShowDialog();
		dialogManager.SetSize(dialogWidth, dialogHeight);
		dialogManager.SetDialogType();
		dialogManager.SetDialogMessage(message);
		dialogManager.SetLeftButtonState(false);
		dialogManager.SetRightButtonState(true, "确定");
		return dialogManager;
	}

	public static DialogManager ShowWarn(string message, int dialogWidth = 0, int dialogHeight = 0)
	{
		DialogManager dialogManager = ShowDialog();
		dialogManager.SetSize(dialogWidth, dialogHeight);
		dialogManager.SetDialogType(WarnType);
		dialogManager.SetDialogMessage(message);
		dialogManager.SetLeftButtonState(false);
		dialogManager.SetRightButtonState(true, "确定");
		return dialogManager;
	}

	public static DialogManager ShowError(string message, int dialogWidth = 700, int dialogHeight = 400)
	{
		DialogManager dialogManager = ShowDialog();
		dialogManager.SetSize(dialogWidth, dialogHeight);
		dialogManager.SetDialogType(ErrorType);
		dialogManager.SetDialogMessage(message);
		dialogManager.SetLeftButtonState(false);
		dialogManager.SetRightButtonState(true, "确定");
		return dialogManager;
	}

	public static DialogManager ShowQuestion(string      message,
											 UnityAction onLeftButtonClick,
											 UnityAction onRightButtonClick,
											 string      leftButtonTxt  = "确定",
											 string      rightButtonTxt = "取消",
											 int         dialogWidth    = 0,
											 int         dialogHeight   = 0)
	{
		DialogManager dialogManager = ShowDialog();
		dialogManager.SetSize(dialogWidth, dialogHeight);
		dialogManager.SetDialogType(QuestionType);
		dialogManager.SetDialogMessage(message);
		dialogManager.SetLeftButtonState(true, leftButtonTxt, onLeftButtonClick);
		dialogManager.SetRightButtonState(true, rightButtonTxt, onRightButtonClick);
		return dialogManager;
	}

	public static DialogManager ShowGetValue(string              valueInfo,
											 string              placeholderText,
											 UnityAction<string> onGetValue,
											 UnityAction         onLeftButtonClick  = null,
											 UnityAction         onRightButtonClick = null,
											 string              leftButtonTxt      = "确定",
											 string              rightButtonTxt     = "取消",
											 int                 dialogWidth        = 360,
											 int                 dialogHeight       = 0)
	{
		DialogManager dialogManager = ShowDialog();
		dialogManager.SetSize(dialogWidth, dialogHeight);
		dialogManager.SetDialogType(GetValue);
		dialogManager.OnGetValue = onGetValue;
		dialogManager.SetValueInfoText(valueInfo);
		dialogManager.SetValuePlaceholder(placeholderText);
		dialogManager.SetLeftButtonState(true, leftButtonTxt, onLeftButtonClick);
		dialogManager.SetRightButtonState(true, rightButtonTxt, onRightButtonClick);
		return dialogManager;
	}

	public void SetSize(int dialogWidth, int dialogHeight)
	{
		dialogWidth = Math.Max(dialogWidth, 280);
		dialogHeight = Math.Max(dialogHeight, 160);
		transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(dialogWidth, dialogHeight);
	}

	public void SetDialogType(int dialogType = InfoType)
	{
		Sprite sprite = null;
		int mainType = 0;
		switch (dialogType)
		{
			case InfoType:
				sprite = InfoIcon;
				mainType = 1;
				break;
			case WarnType:
				sprite = WarnIcon;
				mainType = 1;
				break;
			case ErrorType:
				sprite = ErrorIcon;
				mainType = 1;
				break;
			case QuestionType:
				sprite = QuestionIcon;
				mainType = 1;
				break;
			case GetValue:
				mainType = 2;
				break;
		}
		if (mainType == 0) return;
		DialogTypeIcon.gameObject.SetActive(mainType == 1);
		MessageScrollView.SetActive(mainType == 1);
		ValueInfo.gameObject.SetActive(mainType == 2);
		ValueInputFiled.gameObject.SetActive(mainType == 2);
		if (mainType == 1) DialogTypeIcon.sprite = sprite;
	}

	public void SetDialogMessage(string txt)
	{
		if (string.IsNullOrEmpty(txt)) return;
		DialogMessage.text = txt;
	}

	public void SetLeftButtonState(bool bShow, string txt = "Yes", UnityAction onLeftButtonClick = null)
	{
		LeftButton.gameObject.SetActive(bShow);
		LeftButton.GetComponentInChildren<Text>().text = txt;
		LeftButton.onClick.RemoveAllListeners();
		if (onLeftButtonClick != null) LeftButton.onClick.AddListener(onLeftButtonClick);
		if (OnGetValue != null) LeftButton.onClick.AddListener(() => OnGetValue(ValueInputFiled.text));
		LeftButton.onClick.AddListener(CloseDialog);
	}

	public void SetRightButtonState(bool bShow, string txt = "Yes", UnityAction onRightButtonClick = null)
	{
		RightButton.gameObject.SetActive(bShow);
		RightButton.GetComponentInChildren<Text>().text = txt;
		RightButton.onClick.RemoveAllListeners();
		if (onRightButtonClick != null) RightButton.onClick.AddListener(onRightButtonClick);
		RightButton.onClick.AddListener(CloseDialog);
	}

	public void SetMessageFontSize(int fontSize)
	{
		GetComponent<Text>().fontSize = fontSize;
	}

	public void SetValueInfoText(string txt)
	{
		if (string.IsNullOrEmpty(txt)) return;
		ValueInfo.text = txt;
	}

	public void SetValuePlaceholder(string txt)
	{
		if (string.IsNullOrEmpty(txt)) return;
		ValueInputFiled.placeholder.GetComponent<Text>().text = txt;
		ValueInputFiled.text = string.Empty;
	}
}