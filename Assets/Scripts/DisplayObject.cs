using System.Drawing;
using System;
using UnityEngine;
using Newtonsoft.Json;

namespace Assets.Scripts
{
	public class DisplayObject : Rectangle
	{
		private const int DIGITS = 1;

		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }

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

		public bool InvConvertTo(Transform displayObject)
		{
			if (!displayObject) return false;
			displayObject.name = Name;
			var rect = displayObject.GetComponent<RectTransform>();
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
			return (float)Math.Round(x - GlobalData.OriginPoint.x, DIGITS);
		}

		public static float InvConvertX(float x)
		{
			return (float)Math.Round(x + GlobalData.OriginPoint.x, DIGITS);
		}

		public static float ConvertY(float y)
		{
			return (float)Math.Round(-(y - GlobalData.OriginPoint.y), DIGITS);
		}

		public static float InvConvertY(float y)
		{
			return (float)Math.Round(-(y - GlobalData.OriginPoint.y), DIGITS);
		}

		override public string ToString()
		{
			return $"name: {base.ToString()}";
		}
	}
}