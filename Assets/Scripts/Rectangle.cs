﻿using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts
{
	public class Rectangle
	{
		public Rectangle(float x = 0.0f, float y = 0.0f, float width = 0.0f, float height = 0.0f)
		{
			this.X = x;
			this.Y = y;
			this.Width = width;
			this.Height = height;
		}

		[JsonProperty(PropertyName = "x")]
		public float X { get; set; }

		[JsonProperty(PropertyName = "y")]
		public float Y { get; set; }

		[JsonProperty(PropertyName = "width")]
		public float Width { get; set; }

		[JsonProperty(PropertyName = "height")]
		public float Height { get; set; }

		[JsonIgnoreAttribute]
		public float Left
		{
			get { return X; }
			set
			{
				Width += X - value;
				X = value;
			}
		}

		[JsonIgnoreAttribute]
		public float Right
		{
			get { return X + Width; }
			set { Width = value - X; }
		}

		[JsonIgnoreAttribute]
		public float Top
		{
			get { return Y; }
			set
			{
				Height += Y - value;
				Y = value;
			}
		}

		[JsonIgnoreAttribute]
		public float Bottom
		{
			get { return Y + Height; }
			set { Height = value - Y; }
		}

		public void Set(float x = 0.0f, float y = 0.0f, float width = 0.0f, float height = 0.0f)
		{
			this.X = x;
			this.Y = y;
			this.Width = width;
			this.Height = height;
		}

		public bool IsCrossing(Rectangle rect)
		{
			if (rect == null) return false;
			if (this.Right <= rect.Left || this.Bottom <= rect.Top)
				return false;
			if (this.Left >= rect.Right || this.Top >= rect.Bottom)
				return false;
			return true;
		}

		public bool IsInFrame(Vector2 pos, float frameWidth = 16.0f)
		{
			float halfFrameWidth = frameWidth / 2;
			if (!Contain(this.Left - halfFrameWidth, this.Right + halfFrameWidth, this.Top - halfFrameWidth, this.Bottom + halfFrameWidth, pos))
				return false;
			if (frameWidth > this.Width || frameWidth > this.Height)
				return true;
			if (Contain(this.Left + halfFrameWidth, this.Right - halfFrameWidth, this.Top + halfFrameWidth, this.Bottom - halfFrameWidth, pos))
				return false;
			return true;
		}

		public bool Contain(Vector2 pos)
		{
			return Contain(this.Left, this.Right, this.Top, this.Bottom, pos);
		}

		public static bool Contain(float left, float right, float top, float bottom, Vector2 pos)
		{
			if (pos.x < left) return false;
			if (pos.x > right) return false;
			if (pos.y < top) return false;
			if (pos.y > bottom) return false;
			return true;
		}

		override public string ToString()
		{
			return $"(left: {Left}, right: {Right}, top: {Top}, bottom: {Bottom})";
		}
	}
}
