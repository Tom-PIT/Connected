using Newtonsoft.Json;

namespace TomPIT.Ide.Analysis
{
	internal class Location : ILocation
	{
		[JsonProperty("range")]
		public IRange Range { get; set; }
		[JsonProperty("uri")]
		public string Uri { get; set; }
	}
}