using Newtonsoft.Json;

namespace TomPIT.Design.Services
{
	public class CodeLensCommand : ICodeLensCommand
	{
		[JsonProperty(PropertyName = "title")]
		public string Title { get; set; }

		[JsonProperty(PropertyName = "arguments")]
		public ICodeLensArguments Arguments { get; set; }
	}
}
