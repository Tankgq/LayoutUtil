using System;
using UnityEngine;

namespace Assets.Scripts
{
	public class DisplayObject
	{
		public string name { get; set; }

		public float x { get; set; }

		public float y { get; set; }

		public float width { get; set; }

		public float height { get; set; }

		public bool IsCrossing(DisplayObject displayObject)
		{
			if (displayObject == null) return false;
			if (this.x + this.width <= displayObject.x || this.y + this.height <= displayObject.y)
				return false;
			if (this.x >= displayObject.x + displayObject.width || this.y >= displayObject.y + displayObject.height)
				return false;
			return true;
		}

		public bool Contain(Vector2 pos)
		{
			if (pos.x < this.x) return false;
			if (pos.x > this.x + this.width) return false;
			if (pos.y < this.y) return false;
			if (pos.y > this.y + this.height) return false;
			return true;
		}

		public static DisplayObject ConvertTo(Transform displayObject)
		{
			if (!displayObject) return null;

			var rect = displayObject.GetComponent<RectTransform>();
			var pos = rect.anchoredPosition;
			var size = rect.sizeDelta;

			var result = new DisplayObject
			{
				name = displayObject.name,
				x = ConvertX(pos.x),
				y = ConvertY(pos.y),
				width = size.x,
				height = size.y
			};
			return result;
		}

		public bool InvConvertTo(Transform displayObject)
		{
			if (!displayObject) return false;
			displayObject.name = name;
			var rect = displayObject.GetComponent<RectTransform>();
			rect.sizeDelta = new Vector2(width, height);
			rect.anchoredPosition = new Vector2(InvConvertX(x), InvConvertY(y));
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
			return (float)Math.Round(x - GlobalData.OriginPoint.x, 1);
		}

		public static float InvConvertX(float x)
		{
			return (float)Math.Round(x + GlobalData.OriginPoint.x, 1);
		}

		public static float ConvertY(float y)
		{
			return (float)Math.Round(-(y - GlobalData.OriginPoint.y), 1);
		}

		public static float InvConvertY(float y)
		{
			return (float)Math.Round(-(y - GlobalData.OriginPoint.y), 1);
		}
	}
}