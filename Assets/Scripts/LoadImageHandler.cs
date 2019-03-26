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

    void Start() {
        // Load("X:/Users/TankGq/Desktop/img.jpg", new Vector2(0f, 0f));
        Load("/Users/Tank/Documents/OneDrive/Documents/icon.png", new Vector2(100f, 0f));
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
        Texture2D texture2 = LoadTexture2DByIO(imageUrl);

        Transform imageItem = Instantiate(displayObject, mainContainer);
        
        Image image = imageItem.GetComponent<Image>();
        image.material = new Material(Shader.Find("UI/Default")) {
            mainTexture = texture2
        };
        RectTransform rect = imageItem.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(texture2.width, texture2.height);
        rect.anchoredPosition = pos - mainContainer.GetComponent<RectTransform>().anchoredPosition;
    }
}
