using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class 
    LoadImageHandler : MonoBehaviour
{
    public Transform DisplayObject;
    public Transform MainContainer;

    private static readonly Dictionary<string, Material> MaterialDic = new Dictionary<string, Material>();
    private static readonly Dictionary<string, Vector2> SizeDic = new Dictionary<string, Vector2>();

    private static readonly List<Transform> DisplayObjectPool = new List<Transform>();

    private Transform GetDisplayObject()
    {
        int length = DisplayObjectPool.Count;
        if (length == 0) return Instantiate(DisplayObject, MainContainer);
        Transform result = DisplayObjectPool[length - 1];
        DisplayObjectPool.RemoveAt(length - 1);
        return result;
    }
    
    public void RecycleDisplayObject(Transform displayObject) {
        if (!displayObject) return;
        DisplayObjectManager.DeSelectDisplayObject(displayObject);
        displayObject.SetParent(null);
        DisplayObjectPool.Add(displayObject);
    }

    private void Start() {
#if UNITY_EDITOR
#if UNITY_EDITOR_WIN
        Load("X:/Users/TankGq/Desktop/img.jpg", new Vector2(300f, 300f));
#else
        Load("/Users/Tank/Documents/OneDrive/Documents/icon.png", new Vector2(100f, 0f));
#endif
#endif
    }

    public static Texture2D LoadTexture2DbyIo(string imageUrl)
    {
        byte[] bytes = Utils.ReadFile(imageUrl);
        Texture2D texture2D = new Texture2D((int) GlobalData.DefaultSize.x, (int) GlobalData.DefaultSize.y);
        texture2D.LoadImage(bytes);
        return texture2D;
    }

    public void Load(string imageUrl, Vector2 pos)
    {
        if (!DisplayObject || !MainContainer) return;
        Material material = null;
        Vector2 size;
        if (string.IsNullOrEmpty(imageUrl))
        {
            size = GlobalData.DefaultSize;
        }
        else
        {
            if (MaterialDic.ContainsKey(imageUrl)) {
                material = MaterialDic[imageUrl];
                size = SizeDic[imageUrl];
            } else {
                Texture2D texture2 = LoadTexture2DbyIo(imageUrl);
                material = new Material(Shader.Find("UI/Default")) {
                    mainTexture = texture2
                };
                size = new Vector2(texture2.width, texture2.height);
                MaterialDic[imageUrl] = material;
                SizeDic[imageUrl] = size;
            }
        }
        
        Transform imageElement = GetDisplayObject();
        imageElement.SetParent(MainContainer);
        int instanceId = imageElement.GetInstanceID();
        GlobalData.DisplayObjectPaths[instanceId] = imageUrl;
        GlobalData.DisplayObjects.Add(imageElement);
        Debug.Log($"Load Image: {instanceId}, name: {imageElement.name}");

        imageElement.name = $"{(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds}";
        Image image = imageElement.GetComponent<Image>();
        image.material = material;
        image.color = (material ? Color.white : Color.clear);
        RectTransform rect = imageElement.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(size.x, size.y);
        pos.y = - pos.y;
        rect.anchoredPosition = pos - MainContainer.GetComponent<RectTransform>().anchoredPosition;
    }
}
