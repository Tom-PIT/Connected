using Newtonsoft.Json;
using TomPIT.Middleware;

namespace TomPIT.Navigation
{
	public abstract class SiteMapElement : MiddlewareObject, ISiteMapElement
	{
		public string Text { get; set; }

		[JsonIgnore]
		public ISiteMapElement Parent { get; set; }
	}
}
