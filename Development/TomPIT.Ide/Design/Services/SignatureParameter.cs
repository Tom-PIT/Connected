using Newtonsoft.Json;

namespace TomPIT.Design.Services
{
	internal class SignatureParameter : ISignatureParameter
	{
		[JsonProperty(PropertyName = "label")]
		public string Label { get; set; }

		[JsonProperty(PropertyName = "documentation")]
		public string Documentation { get; set; }
	}
}
