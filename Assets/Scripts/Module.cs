using System.Collections.Generic;
using Newtonsoft.Json;

public class Module {
	[JsonProperty(PropertyName = "name")]
	public string Name;

	[JsonProperty(PropertyName = "elements")]
	public List<Element> Elements;

	[JsonProperty(PropertyName = "width")]
	public float Width;

	[JsonProperty(PropertyName = "height")]
	public float Height;

	[JsonProperty(PropertyName = "x")]
	public float X;

	[JsonProperty(PropertyName = "y")]
	public float Y;
}
