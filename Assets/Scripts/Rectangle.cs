using Newtonsoft.Json;
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
            set { X = value; }
        }

        [JsonIgnoreAttribute]
        public float Right
        {
            get { return X + Width; }
            set { X = value - Width; }
        }

        [JsonIgnoreAttribute]
        public float Top
        {
            get { return Y; }
            set { Y = value; }
        }

        [JsonIgnoreAttribute]
        public float Bottom
        {
            get { return Y + Height; }
            set { Y = value - Height; }
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

        public bool Contain(Vector2 pos)
        {
            if (pos.x < this.Left) return false;
            if (pos.x > this.Right) return false;
            if (pos.y < this.Top) return false;
            if (pos.y > this.Bottom) return false;
            return true;
        }

        override public string ToString() {
            return $"(left: {Left}, right: {Right}, top: {Top}, bottom: {Bottom})";
        }
    }
}
