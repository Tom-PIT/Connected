using Newtonsoft.Json;

namespace TomPIT.Ide.Analysis.Lenses
{
	internal class CodeLensDescriptor : ICodeLensDescriptor
	{
		[JsonProperty(PropertyName = "range")]
		public IRange Range { get; set; }
		[JsonProperty(PropertyName = "id")]
		public string Id { get; set; }
		[JsonProperty(PropertyName = "command")]
		public ICodeLensCommand Command { get; set; }
	}
}
