using Newtonsoft.Json;

namespace TomPIT.Ide.Analysis.Signatures
{
	internal class SignatureParameter : ISignatureParameter
	{
		[JsonProperty(PropertyName = "label")]
		public string Label { get; set; }

		[JsonProperty(PropertyName = "documentation")]
		public string Documentation { get; set; }
	}
}
