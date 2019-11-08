using Newtonsoft.Json;

namespace TomPIT.Ide.Analysis.Hovering
{
	internal class HoverLine : IHoverLine
	{
		[JsonProperty(PropertyName = "value")]
		public string Value { get; set; }
	}
}
