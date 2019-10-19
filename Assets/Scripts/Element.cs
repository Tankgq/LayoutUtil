using System;
using Newtonsoft.Json;
using UnityEngine;

public class Element : Rectangle {
	private const int EnableDigits = 1;

	[JsonProperty(PropertyName = "name")]
	public string Name { get; set; }

	[JsonProperty(PropertyName = "visible")]
	public bool Visible { get; set; }

	public static Element ConvertTo(Transform displayObject) {
		if(! displayObject) return null;

		RectTransform rect = displayObject.GetComponent<RectTransform>();
		Vector2 pos = rect.anchoredPosition;
		Vector2 size = rect.sizeDelta;

		Element result = new Element {
			Name = displayObject.name,
			X = ConvertX(pos.x),
			Y = ConvertY(pos.y),
			Width = size.x,
			Height = size.y,
			Visible = true
		};
		return result;
	}

	public bool InvConvertTo(Transform displayObject) {
		if(displayObject == null) return false;
		displayObject.name = Name;
		RectTransform rect = displayObject.GetComponent<RectTransform>();
		rect.anchoredPosition = new Vector2(InvConvertX(X), InvConvertY(Y));
		rect.sizeDelta = new Vector2(Width, Height);
		displayObject.gameObject.SetActive(Visible);
		return true;
	}

	public static Vector2 ConvertTo(Vector2 pos) {
		return new Vector2(ConvertX(pos.x), ConvertY(pos.y));
	}

	public static Vector2 InvConvertTo(Vector2 pos) {
		return new Vector2(InvConvertX(pos.x), InvConvertY(pos.y));
	}

	public static float ConvertX(float x) {
		return (float)Math.Round(x - GlobalData.OriginPoint.x, EnableDigits);
	}

	public static float InvConvertX(float x) {
		return (float)Math.Round(x + GlobalData.OriginPoint.x, EnableDigits);
	}

	public static float ConvertY(float y) {
		return (float)Math.Round(-(y - GlobalData.OriginPoint.y), EnableDigits);
	}

	public static float InvConvertY(float y) {
		return (float)Math.Round(-(y - GlobalData.OriginPoint.y), EnableDigits);
	}

	public override string ToString() {
		return $"name: {Name}, visible: {Visible}, rect: {base.ToString()}";
	}
}
