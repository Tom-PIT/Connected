using Newtonsoft.Json;

namespace TomPIT.Navigation
{
	public abstract class SiteMapElement : ISiteMapElement
	{
		public string Text { get; set; }

		[JsonIgnore]
		public ISiteMapElement Parent { get; set; }
	}
}
