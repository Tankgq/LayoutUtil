using Newtonsoft.Json;
using UnityEngine;

public class Rectangle {
	public Rectangle(float x = 0.0f, float y = 0.0f, float width = 0.0f, float height = 0.0f) {
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}

	[JsonProperty(PropertyName = "x")]
	public float X { get; set; }

	[JsonProperty(PropertyName = "y")]
	public float Y { get; set; }

	[JsonProperty(PropertyName = "width")]
	public float Width { get; set; }

	[JsonProperty(PropertyName = "height")]
	public float Height { get; set; }

	[JsonIgnore]
	public float Left {
		get { return X; }
		set {
			Width += X - value;
			X = value;
		}
	}

	[JsonIgnore]
	public float Right {
		get { return X + Width; }
		set { Width = value - X; }
	}

	[JsonIgnore]
	public float Top {
		get { return Y; }
		set {
			Height += Y - value;
			Y = value;
		}
	}

	[JsonIgnore]
	public float Bottom {
		get { return Y + Height; }
		set { Height = value - Y; }
	}

	public float HorizontalCenter {
		get { return Y + Height * 0.5f; }
		set { Height = (value - Y) * 2; }
	}

	public float VerticalCenter {
		get { return X + Width * 0.5f; }
		set { Width = (value - X) * 2; }
	}

	public void Set(float x = 0.0f, float y = 0.0f, float width = 0.0f, float height = 0.0f) {
		X = x;
		Y = y;
		Width = width;
		Height = height;
	}

	public bool IsCrossing(Rectangle rect) {
		if(rect == null) return false;
		if(Right <= rect.Left || Bottom <= rect.Top) return false;
		return ! (Left >= rect.Right) && ! (Top >= rect.Bottom);
	}

	public bool IsInFrame(Vector2 pos, float frameWidth = 16.0f) {
		float halfFrameWidth = frameWidth / 2;
		if(! Contain(Left - halfFrameWidth, Right + halfFrameWidth, Top - halfFrameWidth, Bottom + halfFrameWidth, pos)) return false;
		if(frameWidth > Width || frameWidth > Height) return true;
		return ! Contain(Left + halfFrameWidth, Right - halfFrameWidth, Top + halfFrameWidth, Bottom - halfFrameWidth, pos);
	}

	public bool Contain(Vector2 pos) {
		return Contain(Left, Right, Top, Bottom, pos);
	}

	public static bool Contain(float left, float right, float top, float bottom, Vector2 pos) {
		if(pos.x < left) return false;
		if(pos.x > right) return false;
		if(pos.y < top) return false;
		return ! (pos.y > bottom);
	}

	public override string ToString() {
		return $"(left: {Left}, right: {Right}, top: {Top}, bottom: {Bottom})";
	}
}
