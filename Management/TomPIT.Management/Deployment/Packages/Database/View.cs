using Newtonsoft.Json;
using TomPIT.Deployment.Database;

namespace TomPIT.Management.Deployment.Packages.Database
{
	public class View : SchemaBase, IView
	{
		[JsonProperty(PropertyName = "definition")]
		public string Definition { get; set; }
	}
}
