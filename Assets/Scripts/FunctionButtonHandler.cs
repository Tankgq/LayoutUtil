using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class FunctionButtonHandler : MonoBehaviour
{
    public ContainerManager ContainerManager;
    public RectTransform ContainerRect;
    public Slider ScaleSlider;

    public void OnAddButtonClick()
    {
        ContainerManager.AddDisplayObject(null, Vector2.zero, GlobalData.DefaultSize);
    }

    public void OnRemoveButtonClick()
    {
        ContainerManager.RemoveSelectedDisplayObject();
    }

    public void OnUpButtonClick()
    {
        if (GlobalData.CurrentSelectDisplayObjectDic.Count != 1) return;
        string displayObjectKey = GlobalData.CurrentSelectDisplayObjectDic.First().Key;
        string displayObjectName = Utils.GetDisplayObjectName(displayObjectKey);
        int idx = GlobalData.CurrentDisplayObjects.FindIndex(element => element.name.Equals(displayObjectKey));
        if (idx <= 0 || idx >= GlobalData.CurrentDisplayObjects.Count) return;
        Transform tmp = GlobalData.CurrentDisplayObjects[idx];
        GlobalData.CurrentDisplayObjects[idx] = GlobalData.CurrentDisplayObjects[idx - 1];
        GlobalData.CurrentDisplayObjects[idx - 1] = tmp;
        tmp.SetSiblingIndex(idx - 1);
        List<DisplayObject> displayObjectDataList = GlobalData.Modules[GlobalData.CurrentModule];
        DisplayObject tmp2 = displayObjectDataList[idx];
        displayObjectDataList[idx] = displayObjectDataList[idx - 1];
        displayObjectDataList[idx - 1] = tmp2;
    }

    public void OnDownButtonClick()
    {
        if (GlobalData.CurrentSelectDisplayObjectDic.Count != 1) return;
        string displayObjectKey = GlobalData.CurrentSelectDisplayObjectDic.First().Key;
        string displayObjectName = Utils.GetDisplayObjectName(displayObjectKey);
        int idx = GlobalData.CurrentDisplayObjects.FindIndex(element => element.name.Equals(displayObjectKey));
        if (idx < 0 || idx >= GlobalData.CurrentDisplayObjects.Count - 1) return;
        Transform tmp = GlobalData.CurrentDisplayObjects[idx];
        GlobalData.CurrentDisplayObjects[idx] = GlobalData.CurrentDisplayObjects[idx + 1];
        GlobalData.CurrentDisplayObjects[idx + 1] = tmp;
        tmp.SetSiblingIndex(idx + 1);
        List<DisplayObject> displayObjectDataList = GlobalData.Modules[GlobalData.CurrentModule];
        DisplayObject tmp2 = displayObjectDataList[idx];
        displayObjectDataList[idx] = displayObjectDataList[idx + 1];
        displayObjectDataList[idx + 1] = tmp2;
    }

    public void OnImportButtonClick()
    {
        string filePath = OpenFileUtil.OpenFile("json 文件(*.json)\0*.json");
        if (string.IsNullOrEmpty(filePath)) return;
        byte[] bytes = Utils.ReadFile(filePath);
        string jsonStr = System.Text.Encoding.UTF8.GetString(bytes);
        try
        {
            List<DisplayObject> displayObjects = JsonConvert.DeserializeObject<List<DisplayObject>>(jsonStr);
            foreach (DisplayObject displayObject in displayObjects)
            {
                ContainerManager.AddDisplayObject(null,
                    new Vector2(displayObject.X + GlobalData.OriginPoint.x, displayObject.Y + GlobalData.OriginPoint.y), 
                    new Vector2(displayObject.Width, displayObject.Height),
                    displayObject.Name);
            }
        }
        catch (Exception e)
        {
            DialogManager.ShowError($"导入失败({e})");
        }
    }

    public void OnExportButtonClick()
    {
        string filePath = SaveFileUtil.SaveFile("json 文件(*.json)\0*.json");
        if (string.IsNullOrEmpty(filePath)) return;
        List<DisplayObject> displayObjects = new List<DisplayObject>();
        foreach (Transform displayObject in GlobalData.CurrentDisplayObjects)
        {
            displayObjects.Add(DisplayObject.ConvertTo(displayObject));
        }
        string jsonString = JsonConvert.SerializeObject(displayObjects, Formatting.Indented);
        bool result = Utils.WriteFile(filePath, System.Text.Encoding.UTF8.GetBytes(jsonString));
        if (result) DialogManager.ShowInfo($"成功导出到 {filePath}");
        else DialogManager.ShowError($"导出失败", 0, 0);
    }

    public void OnCreateModuleButtonClick() {
        ContainerManager.CreateModule();
    }

    public void OnScaleSliderValueChanged(float value)
    {
        value /= 10;
        ContainerRect.localScale = new Vector3(value, value, value);
        ScaleSlider.GetComponentInChildren<Text>().text = $"x{value:0.0}";
    }

    private void Start()
    {
        ScaleSlider.OnValueChangedAsObservable()
//            .Sample(TimeSpan.FromMilliseconds(500))
            .Subscribe(OnScaleSliderValueChanged);
    }
}