using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class ContainerManager : MonoBehaviour
{
    private static readonly Dictionary<string, Material> MaterialDic = new Dictionary<string, Material>();
    private static readonly Dictionary<string, Vector2> SizeDic = new Dictionary<string, Vector2>();

    private static readonly List<Transform> DisplayObjectPool = new List<Transform>();

    private Transform GetDisplayObject() {
        int length = DisplayObjectPool.Count;
        if (length == 0) return Instantiate(GlobalData.DisplayObjectPrefab.transform, this.transform);
        Transform result = DisplayObjectPool[length - 1];
        DisplayObjectPool.RemoveAt(length - 1);
        return result;
    }

    public void RecycleDisplayObject(Transform displayObject) {
        if (!displayObject) return;
        DisplayObjectManager.DeSelectDisplayObject(displayObject);
        displayObject.GetComponent<Toggle>().isOn = false;
        displayObject.SetParent(null);
        DisplayObjectPool.Add(displayObject);
    }

    private void Start() {
#if UNITY_EDITOR
#if UNITY_EDITOR_WIN
        AddDisplayObject("X:/Users/TankGq/Desktop/img.jpg", new Vector2(300f, 300f), Vector2.zero);
#else
        AddDisplayObject("/Users/Tank/Documents/OneDrive/Documents/icon.png", new Vector2(100f, 0f), Vector2.zero);
#endif
#endif
        GlobalData.CurrentSelectDisplayObjects.ObserveEveryValueChanged(dic => dic.Count)
            .Subscribe(count => {
                foreach (Transform displayObjectItem in GlobalData.DisplayObjects)
                    displayObjectItem.GetComponent<Toggle>().isOn = false;
                if (count == 0) return;
                foreach (var pair in GlobalData.CurrentSelectDisplayObjects) {
                    pair.Value.GetComponent<Toggle>().isOn = true;
                }
            });
    }

    public static Texture2D LoadTexture2DbyIo(string imageUrl) {
        byte[] bytes = Utils.ReadFile(imageUrl);
        Texture2D texture2D = new Texture2D((int)GlobalData.DefaultSize.x, (int)GlobalData.DefaultSize.y);
        texture2D.LoadImage(bytes);
        return texture2D;
    }

    public Transform AddDisplayObject(string imageUrl, Vector2 pos, Vector2 size, string elementName = null) {
        Material material = null;
        if (!string.IsNullOrEmpty(imageUrl)) {
            if (MaterialDic.ContainsKey(imageUrl)) {
                material = MaterialDic[imageUrl];
                if (size == Vector2.zero) size = SizeDic[imageUrl];
            } else {
                Texture2D texture2 = LoadTexture2DbyIo(imageUrl);
                material = new Material(Shader.Find("UI/Default")) {
                    mainTexture = texture2
                };
                MaterialDic[imageUrl] = material;
                SizeDic[imageUrl] = new Vector2(texture2.width, texture2.height);
                if (size == Vector2.zero) size = SizeDic[imageUrl];
            }
        }
        Transform imageElement = GetDisplayObject();
        imageElement.SetParent(this.transform);
        int instanceId = imageElement.GetInstanceID();
        GlobalData.DisplayObjectPaths[instanceId] = imageUrl;
        GlobalData.DisplayObjects.Add(imageElement);
        imageElement.name = string.IsNullOrEmpty(elementName)
            ? (string.IsNullOrEmpty(imageUrl) ? GlobalData.DefaultName + (++GlobalData.NameId) : Utils.GetFileNameInPath(imageUrl))
            : elementName;
        Image image = imageElement.GetComponent<Image>();
        image.material = material;
        image.color = (material ? Color.white : Color.clear);
        RectTransform rect = imageElement.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(size.x, size.y);
        pos += GlobalData.OriginPoint;
        pos.y = - pos.y;
        rect.anchoredPosition = pos;
        return imageElement;
    }
    
    public void RemoveSelectedDisplayObject()
    {
        var count = GlobalData.CurrentSelectDisplayObjects.Count;
        if (count == 0) {
//            MessageBoxUtil.Show("请先选择要删除的对象");
            DialogManager.ShowInfo("请先选择要删除的对象");
            return;
        }

        var length = GlobalData.DisplayObjects.Count;
        foreach (var pair in GlobalData.CurrentSelectDisplayObjects) {
            RecycleDisplayObject(pair.Value);
            if (GlobalData.DisplayObjectPaths.ContainsKey(pair.Key))
                GlobalData.DisplayObjectPaths.Remove(pair.Key);
            var idx = GlobalData.DisplayObjects.FindIndex(0, element => element.GetInstanceID() == pair.Key);
            if (idx < 0 || idx >= length) continue;
            GlobalData.DisplayObjects.RemoveAt(idx);
            --length;
        }

        GlobalData.CurrentSelectDisplayObjects.Clear();
    }
}
