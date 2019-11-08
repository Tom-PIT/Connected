using System.Collections.Generic;

namespace TomPIT.Ide.TextServices.Languages
{
	public interface ISignatureHelp
	{
		int ActiveParameter { get; }
		int ActiveSignature { get; }

		List<ISignatureInformation> Signatures { get; }
	}
}
