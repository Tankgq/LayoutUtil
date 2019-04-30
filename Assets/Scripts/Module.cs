using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Assets.Scripts
{
	public class Module
	{
		[JsonProperty(PropertyName = "name")]
		public string Name;

		[JsonProperty(PropertyName = "elements")]
		public List<Element> Elements;

		[JsonProperty(PropertyName = "width")]
		public float Width = 0.0f;

		[JsonProperty(PropertyName = "height")]
		public float Height = 0.0f;

		[JsonProperty(PropertyName = "x")]
		public float X = 0.0f;

		[JsonProperty(PropertyName = "y")]
		public float Y = 0.0f;
	}
}
