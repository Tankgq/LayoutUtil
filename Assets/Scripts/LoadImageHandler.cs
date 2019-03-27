using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class 
    LoadImageHandler : MonoBehaviour
{
    public Transform displayObject;
    public Transform mainContainer;
    public Vector2 containerPos;
    public Transform originPoint;
    public Canvas mainCanvas;

    private static Dictionary<string, Material> _materialDic = new Dictionary<string, Material>();
    private static Dictionary<string, KeyValuePair<int, int>> _sizeDic = new Dictionary<string, KeyValuePair<int, int>>();

    void Start() {
#if UNITY_EDITOR
        // Load("X:/Users/TankGq/Desktop/img.jpg", new Vector2(300f, 300f));
        Load("/Users/Tank/Documents/OneDrive/Documents/icon.png", new Vector2(100f, 0f));
#endif
    }

    public static Texture2D LoadTexture2DByIO(string imageUrl)
    {
        byte[] bytes;
        using (FileStream fs = new FileStream(imageUrl, FileMode.Open, FileAccess.Read)) {
            fs.Seek(0, SeekOrigin.Begin);
            bytes = new byte[fs.Length];
            fs.Read(bytes, 0, (int)fs.Length);
            fs.Close();
            fs.Dispose();
        }
        Texture2D texture2D = new Texture2D(1, 1);
        texture2D.LoadImage(bytes);
        return texture2D;
    }

    public void Load(string imageUrl, Vector2 pos)
    {
        if (!displayObject || !mainContainer || string.IsNullOrEmpty(imageUrl)) return;
        Material material = null;
        KeyValuePair<int, int> sizePair;
        if (_materialDic.ContainsKey(imageUrl))
        {
            material = _materialDic[imageUrl];
            sizePair = _sizeDic[imageUrl];
        }
        else
        {
            Texture2D texture2 = LoadTexture2DByIO(imageUrl);
            material = new Material(Shader.Find("UI/Default")) {
                mainTexture = texture2
            };
            sizePair = new KeyValuePair<int, int>(texture2.width, texture2.height);
            _materialDic[imageUrl] = material;
            _sizeDic[imageUrl] = sizePair;
        }
        
        Transform imageElement = Instantiate(displayObject, mainContainer);
        imageElement.name = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0)).TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
        Image image = imageElement.GetComponent<Image>();
        image.material = material;
        RectTransform rect = imageElement.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(sizePair.Key, sizePair.Value);
        pos.y = - pos.y;
        rect.anchoredPosition = pos - mainContainer.GetComponent<RectTransform>().anchoredPosition;
        Debug.Log($"pos: {pos}, anchoredPosition: {mainContainer.GetComponent<RectTransform>().anchoredPosition}");
    }
}
