using Newtonsoft.Json;

namespace TomPIT.Design.Services
{
	internal class Location : ILocation
	{
		[JsonProperty("range")]
		public IRange Range { get; set; }
		[JsonProperty("uri")]
		public string Uri { get; set; }
	}
}