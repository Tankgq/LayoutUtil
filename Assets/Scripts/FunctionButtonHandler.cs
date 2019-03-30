using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class FunctionButtonHandler : MonoBehaviour
{
    public ContainerManager ContainerManager;

    public void OnAddButtonClick()
    {
        ContainerManager.AddDisplayObject(null, GlobalData.OriginPoint, GlobalData.DefaultSize);
    }

    public void OnRemoveButtonClick()
    {
        ContainerManager.RemoveSelectedDisplayObject();
    }

    public void OnUpButtonClick()
    {
        if (GlobalData.CurrentSelectDisplayObjects.Count != 1) return;
        int instanceId = GlobalData.CurrentSelectDisplayObjects.Keys.First();
        int idx = GlobalData.DisplayObjects.FindIndex(element => element.GetInstanceID() == instanceId);
        if (idx <= 0 || idx >= GlobalData.DisplayObjects.Count) return;
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
        if (idx < 0 || idx >= GlobalData.DisplayObjects.Count - 1) return;
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
                    new Vector2(DisplayObject.InvConvertX(displayObject.X), DisplayObject.InvConvertY(displayObject.Y)),
                    new Vector2(displayObject.Width, displayObject.Height),
                    displayObject.Name);
            }
        }
        catch (Exception e)
        {
            MessageBoxUtil.Show($"导入失败({e})");
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
        string jsonString = JsonConvert.SerializeObject(displayObjects);
        bool result = Utils.WriteFile(filePath, System.Text.Encoding.UTF8.GetBytes(jsonString));
        if (result) MessageBoxUtil.Show($"成功导出到 {filePath}");
        else MessageBoxUtil.Show($"导出失败");
    }
}