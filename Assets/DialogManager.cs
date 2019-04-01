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
    
    public Sprite InfoIcon;
    public Sprite WarnIcon;
    public Sprite ErrorIcon;
    public Sprite QuestionIcon;

    public Image DialogTypeIcon;
    public Text DialogMessage;

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

    public static void RecycleDialog(Transform dialog) {
        if (!dialog) return;
        dialog.gameObject.SetActive(false);
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

    public static DialogManager ShowError(string message, int dialogWidth = 700, int dialogHeight = 400) {
        DialogManager dialogManager = ShowDialog();
        dialogManager.SetSize(dialogWidth, dialogHeight);
        dialogManager.SetDialogType(ErrorType);
        dialogManager.SetDialogMessage(message);
        dialogManager.SetLeftButtonState(false);
        dialogManager.SetRightButtonState(true, "确定");
        return dialogManager;
    }

    public static DialogManager ShowQuestion(string message,
                                             UnityAction onLeftButtonClick,
                                             UnityAction onRightButtonClick,
                                             string leftButtonTxt = "确定",
                                             string rightButtonTxt = "取消",
                                             int dialogWidth = 0,
                                             int dialogHeight = 0) {
        DialogManager dialogManager = ShowDialog();
        dialogManager.SetSize(dialogWidth, dialogHeight);
        dialogManager.SetDialogType(ErrorType);
        dialogManager.SetDialogMessage(message);
        dialogManager.SetLeftButtonState(true, leftButtonTxt, onLeftButtonClick);
        dialogManager.SetRightButtonState(true, rightButtonTxt, onRightButtonClick);
        return dialogManager;
    }

    public void SetSize(int dialogWidth, int dialogHeight)
    {
        dialogWidth = Math.Max(dialogWidth, 280);
        dialogHeight = Math.Max(dialogHeight, 160);
        GetComponent<RectTransform>().sizeDelta = new Vector2(dialogWidth, dialogHeight);
    }

    public void SetDialogType(int dialogType = InfoType)
    {
        Sprite sprite = null;
        switch (dialogType)
        {
            case InfoType:
                sprite = InfoIcon;
                break;
            case WarnType:
                sprite = WarnIcon;
                break;
            case ErrorType:
                sprite = ErrorIcon;
                break;
            case QuestionType:
                sprite = QuestionIcon;
                break;
        }

        DialogTypeIcon.sprite = sprite;
    }

    public void SetDialogMessage(string txt)
    {
        DialogMessage.text = txt;
    }

    public void SetLeftButtonState(bool bShow, string txt = "Yes", UnityAction onLeftButtonClick = null)
    {
        LeftButton.gameObject.SetActive(bShow);
        LeftButton.GetComponentInChildren<Text>().text = txt;
        LeftButton.onClick.RemoveAllListeners();
        if(onLeftButtonClick != null) LeftButton.onClick.AddListener(onLeftButtonClick);
        LeftButton.onClick.AddListener(CloseDialog);
    }

    public void SetRightButtonState(bool bShow, string txt = "Yes", UnityAction onRightButtonClick = null) {
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
}
