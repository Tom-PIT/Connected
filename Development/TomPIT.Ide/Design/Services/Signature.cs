using System.Collections.Generic;
using Newtonsoft.Json;

namespace TomPIT.Design.Services
{
	internal class Signature : ISignature
	{
		private List<ISignatureParameter> _parameters = null;

		[JsonProperty(PropertyName = "label")]
		public string Label { get; set; }
		[JsonProperty(PropertyName = "documentation")]
		public string Documentation { get; set; }
		[JsonProperty(PropertyName = "parameters")]
		public List<ISignatureParameter> Parameters
		{
			get
			{
				if (_parameters == null)
					_parameters = new List<ISignatureParameter>();

				return _parameters;
			}
		}
	}
}
