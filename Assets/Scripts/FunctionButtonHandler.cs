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
        if (GlobalData.CurrentSelectDisplayObjects.Count != 1) return;
        int instanceId = GlobalData.CurrentSelectDisplayObjects.First().Key;
        int idx = GlobalData.DisplayObjects.FindIndex(element => element.GetInstanceID() == instanceId);
        Debug.Log($"[OnUpButtonClick] current idx: {idx}");
        if (idx <= 0 || idx >= GlobalData.DisplayObjects.Count)
        {
            Debug.Log("[OnUpButtonClick] return");
            return;
        }
        Transform tmp = GlobalData.DisplayObjects[idx];
        GlobalData.DisplayObjects[idx] = GlobalData.DisplayObjects[idx - 1];
        GlobalData.DisplayObjects[idx - 1] = tmp;
        tmp.SetSiblingIndex(idx - 1);
    }

    public void OnDownButtonClick()
    {
        if (GlobalData.CurrentSelectDisplayObjects.Count != 1) return;
        int instanceId = GlobalData.CurrentSelectDisplayObjects.Keys.First();
        int idx = GlobalData.DisplayObjects.FindIndex(element => element.GetInstanceID() == instanceId);
        Debug.Log($"[OnDownButtonClick] current idx: {idx}");
        if (idx < 0 || idx >= GlobalData.DisplayObjects.Count - 1)
        {
            Debug.Log("[OnDownButtonClick] return");
            return;
        }
        Transform tmp = GlobalData.DisplayObjects[idx];
        GlobalData.DisplayObjects[idx] = GlobalData.DisplayObjects[idx + 1];
        GlobalData.DisplayObjects[idx + 1] = tmp;
        tmp.SetSiblingIndex(idx + 1);
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
        foreach (Transform displayObject in GlobalData.DisplayObjects)
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