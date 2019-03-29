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
        Load("X:/Users/TankGq/Desktop/img.jpg", new Vector2(300f, 300f), Vector2.zero);
#else
        Load("/Users/Tank/Documents/OneDrive/Documents/icon.png", new Vector2(100f, 0f), Vector2.zero);
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

    public Transform Load(string imageUrl, Vector2 pos, Vector2 size, string elementName = null)
    {
        if (!DisplayObject || !MainContainer) return null;
        Material material = null;
        if (! string.IsNullOrEmpty(imageUrl))
        {
            if (MaterialDic.ContainsKey(imageUrl)) {
                material = MaterialDic[imageUrl];
                if(size == Vector2.zero) size = SizeDic[imageUrl];
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
        imageElement.SetParent(MainContainer);
        int instanceId = imageElement.GetInstanceID();
        GlobalData.DisplayObjectPaths[instanceId] = imageUrl;
        GlobalData.DisplayObjects.Add(imageElement);
        if(! string.IsNullOrEmpty(imageUrl))
            imageElement.name = string.IsNullOrEmpty(elementName) ? Utils.GetFileNameInPath(imageUrl) : elementName;
        Image image = imageElement.GetComponent<Image>();
        image.material = material;
        image.color = (material ? Color.white : Color.clear);
        RectTransform rect = imageElement.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(size.x, size.y);
        pos.y = - pos.y;
        rect.anchoredPosition = pos - MainContainer.GetComponent<RectTransform>().anchoredPosition;
        return imageElement;
    }
}
