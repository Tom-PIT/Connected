using Newtonsoft.Json;

namespace TomPIT.Ide.Analysis.Lenses
{
	public class CodeLensArguments : ICodeLensArguments
	{
		public const string InternalLink = "1";
		public const string ExternalLink = "2";

		[JsonProperty(PropertyName = "microService")]
		public string MicroService { get; set; }
		[JsonProperty(PropertyName = "component")]
		public string Component { get; set; }
		[JsonProperty(PropertyName = "element")]
		public string Element { get; set; }
		[JsonProperty(PropertyName = "kind")]
		public string Kind { get; set; }
	}
}
