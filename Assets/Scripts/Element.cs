using System;
using Newtonsoft.Json;
using UnityEngine;

public class Element : Rectangle
{
	private const int Digits = 1;

	[JsonProperty(PropertyName = "name")]
	public string Name { get; set; }

	[JsonProperty(PropertyName = "visible")]
	public bool Visible { get; set; }

	public static Element ConvertTo(Transform displayObject)
	{
		if (!displayObject) return null;

		RectTransform rect = displayObject.GetComponent<RectTransform>();
		Vector2 pos = rect.anchoredPosition;
		Vector2 size = rect.sizeDelta;

		Element result = new Element
		{
			Name = displayObject.name,
			X = ConvertX(pos.x),
			Y = ConvertY(pos.y),
			Width = size.x,
			Height = size.y,
			Visible = true
		};
		return result;
	}

	public bool InvConvertTo(Transform displayObject)
	{
		if (!displayObject) return false;
		displayObject.name = Name;
		RectTransform rect = displayObject.GetComponent<RectTransform>();
		rect.sizeDelta = new Vector2(Width, Height);
		rect.anchoredPosition = new Vector2(InvConvertX(X), InvConvertY(Y));
		return true;
	}

	public static Vector2 ConvertTo(Vector2 pos)
	{
		return new Vector2(ConvertX(pos.x), ConvertY(pos.y));
	}

	public static Vector2 InvConvertTo(Vector2 pos)
	{
		return new Vector2(InvConvertX(pos.x), InvConvertY(pos.y));
	}

	public static float ConvertX(float x)
	{
		return (float)Math.Round(x - GlobalData.OriginPoint.x, Digits);
	}

	public static float InvConvertX(float x)
	{
		return (float)Math.Round(x + GlobalData.OriginPoint.x, Digits);
	}

	public static float ConvertY(float y)
	{
		return (float)Math.Round(-(y - GlobalData.OriginPoint.y), Digits);
	}

	public static float InvConvertY(float y)
	{
		return (float)Math.Round(-(y - GlobalData.OriginPoint.y), Digits);
	}

	public override string ToString()
	{
		return $"name: {base.ToString()}";
	}
}