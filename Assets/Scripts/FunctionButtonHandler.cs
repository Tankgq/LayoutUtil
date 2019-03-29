using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

public class FunctionButtonHandler : MonoBehaviour
{
    public LoadImageHandler LoadImageUtil;

    public void OnAddButtonClick()
    {
        LoadImageUtil.Load(null, GlobalData.OriginPoint, GlobalData.DefaultSize);
    }

    public void OnRemoveButtonClick()
    {
        var count = GlobalData.CurrentSelectDisplayObjects.Count;
        if (count == 0)
        {
            MessageBoxUtil.Show("请先选择要删除的对象");
            return;
        }

        var length = GlobalData.DisplayObjects.Count;
        foreach (var pair in GlobalData.CurrentSelectDisplayObjects)
        {
            LoadImageUtil.RecycleDisplayObject(pair.Value);
            if (GlobalData.DisplayObjectPaths.ContainsKey(pair.Key))
                GlobalData.DisplayObjectPaths.Remove(pair.Key);
            var idx = GlobalData.DisplayObjects.FindIndex(0, element => element.GetInstanceID() == pair.Key);
            if (idx < 0 || idx >= length) continue;
            GlobalData.DisplayObjects.RemoveAt(idx);
            --length;
        }

        GlobalData.CurrentSelectDisplayObjects.Clear();
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
    }

    public void OnImportButtonClick()
    {
        string filePath = OpenFileUtil.OpenFile("json 文件(*.json)\0*.json");
        byte[] bytes = Utils.ReadFile(filePath);
        string jsonStr = System.Text.Encoding.UTF8.GetString(bytes);
        try
        {
            List<DisplayObject> displayObjects = JsonConvert.DeserializeObject<List<DisplayObject>>(jsonStr);
            foreach (DisplayObject displayObject in displayObjects)
            {
                LoadImageUtil.Load(null,
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