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
            X = ConvertX(pos.x),
            Y = ConvertY(pos.y),
            Width = size.x,
            Height = size.y
        };
        return result;
    }

    public bool InvConvertTo(Transform displayObject) {
        if(! displayObject) return false;
        displayObject.name = Name;
        var rect = displayObject.GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(Width, Height);
        Debug.Log($"x: {X}, invX: {InvConvertX(X)}");
        Debug.Log($"x: {Y}, invX: {InvConvertY(Y)}");
        rect.anchoredPosition = new Vector2(InvConvertX(X), InvConvertY(Y));
        return true;
    }

    public static Vector2 ConvertTo(Vector2 pos)
    {
        return new Vector2(ConvertX(pos.x), ConvertY(pos.y));
    }

    public static float ConvertX(float x)
    {
        return x - GlobalData.OriginPoint.x;
    }

    public static float InvConvertX(float x)
    {
        return x + GlobalData.OriginPoint.x;
    }

    public static float ConvertY(float y)
    {
        return -(y + GlobalData.OriginPoint.y);
    }

    public static float InvConvertY(float y)
    {
        return -(y + GlobalData.OriginPoint.y);
    }
}