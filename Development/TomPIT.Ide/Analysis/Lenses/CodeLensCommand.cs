using Newtonsoft.Json;

namespace TomPIT.Ide.Analysis.Lenses
{
	public class CodeLensCommand : ICodeLensCommand
	{
		[JsonProperty(PropertyName = "title")]
		public string Title { get; set; }

		[JsonProperty(PropertyName = "arguments")]
		public ICodeLensArguments Arguments { get; set; }
	}
}
