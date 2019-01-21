using Newtonsoft.Json;
using TomPIT.Data.DataProviders.Deployment;

namespace TomPIT.Deployment
{
	public class View : SchemaBase, IView
	{
		[JsonProperty(PropertyName = "definition")]
		public string Definition { get; set; }
	}
}
