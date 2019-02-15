using Newtonsoft.Json;
using TomPIT.Deployment.Database;

namespace TomPIT.Management.Deployment
{
	public class View : SchemaBase, IView
	{
		[JsonProperty(PropertyName = "definition")]
		public string Definition { get; set; }
	}
}
