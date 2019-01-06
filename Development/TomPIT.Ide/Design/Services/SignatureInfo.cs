using System.Collections.Generic;
using Newtonsoft.Json;

namespace TomPIT.Design.Services
{
	internal class SignatureInfo : ISignatureInfo
	{
		private List<ISignature> _signatures = null;

		[JsonProperty(PropertyName = "signatures")]
		public List<ISignature> Signatures
		{
			get
			{
				if (_signatures == null)
					_signatures = new List<ISignature>();

				return _signatures;
			}
		}

		[JsonProperty(PropertyName = "activeSignature")]
		public int ActiveSignature { get; set; }
		[JsonProperty(PropertyName = "activeParameter")]
		public int ActiveParameter { get; set; }
	}
}
