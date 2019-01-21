using Newtonsoft.Json;

namespace TomPIT.Deployment
{
	public class Dependency
	{
		[JsonProperty(PropertyName = "name")]
		public string Name { get; set; }
	}
}
