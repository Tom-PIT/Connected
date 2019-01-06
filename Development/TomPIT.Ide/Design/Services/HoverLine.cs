using Newtonsoft.Json;

namespace TomPIT.Design.Services
{
	internal class HoverLine : IHoverLine
	{
		[JsonProperty(PropertyName = "value")]
		public string Value { get; set; }
	}
}
