using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

public class FunctionButtonHandler : MonoBehaviour
{
    public LoadImageHandler LoadImageUtil;

    public void OnAddButtonClick()
    {
        LoadImageUtil.Load(null, GlobalData.OriginPoint);
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
            Debug.Log($"remove instanceId: {pair.Key}");
            var idx = GlobalData.DisplayObjects.FindIndex(0, element => element.GetInstanceID() == pair.Key);
            if (idx < 0 || idx >= length) continue;
            GlobalData.DisplayObjects.RemoveAt(idx);
            --length;
        }

        GlobalData.CurrentSelectDisplayObjects.Clear();
    }

    public void OnImportButtonClick()
    {
        string filePath = OpenFileUtil.OpenFile("json 文件(*.json)\0*.json");
        byte[] bytes = Utils.ReadFile(filePath);
        string jsonStr = System.Text.Encoding.UTF8.GetString(bytes);
        Debug.Log(jsonStr);
        List<DisplayObject> displayObjects = JsonConvert.DeserializeObject<List<DisplayObject>>(jsonStr);
        
        Debug.Log(displayObjects);
    }

    public void OnExportButtonClick()
    {
        List<DisplayObject> displayObjects = new List<DisplayObject>();
        foreach (Transform displayObject in GlobalData.DisplayObjects)
        {
            displayObjects.Add(DisplayObject.ConvertTo(displayObject));
        }
        string jsonString = JsonConvert.SerializeObject(displayObjects);
        Debug.Log(jsonString);
    }
}