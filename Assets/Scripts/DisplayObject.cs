using UnityEngine;

public class DisplayObject
{
    public string Name { get; set; }
    
    public float X { get; set; }
    
    public float Y { get; set; }
    
    public float Width { get; set; }
    
    public float Height { get; set; }

    public static DisplayObject ConvertTo(Transform displayObject)
    {
        if (!displayObject) return null;

        var rect = displayObject.GetComponent<RectTransform>();
        var pos = rect.anchoredPosition;
        var size = rect.sizeDelta;

        var result = new DisplayObject
        {
            Name = displayObject.name,
            X = pos.x,
            Y = -pos.y,
            Width = size.x,
            Height = size.y
        };
        return result;
    }

    public static Vector2 ConvertTo(Vector2 pos)
    {
        return new Vector2(pos.x, -pos.y);
    }

    public static float ConvertX(float x)
    {
        return x;
    }

    public static float InvConvertX(float x)
    {
        return x;
    }

    public static float ConvertY(float y)
    {
        return -y;
    }

    public static float InvConvertY(float y)
    {
        return -y;
    }
}