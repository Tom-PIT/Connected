using Newtonsoft.Json;

namespace TomPIT.Navigation
{
	public abstract class SiteMapElement : ISiteMapElement
	{
		public string Text { get; set; }

		[JsonIgnore]
		public ISiteMapElement Parent { get; set; }

		public bool Visible { get; set; } = true;

		public string Category { get; set; }

		public string Glyph { get; set; }

		public string Css { get; set; }

		public int Priority { get; set; }
	}
}
