using System.Collections.Generic;

namespace TomPIT.Ide.TextServices.Languages
{
	public class SignatureHelp : ISignatureHelp
	{
		private List<ISignatureInformation> _signatures = null;
		public int ActiveParameter { get; set; }

		public int ActiveSignature { get; set; }

		public List<ISignatureInformation> Signatures
		{
			get
			{
				if (_signatures == null)
					_signatures = new List<ISignatureInformation>();

				return _signatures;
			}
		}
	}
}
