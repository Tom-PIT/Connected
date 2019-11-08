using System.Collections.Generic;

namespace TomPIT.Ide.Analysis.Signatures
{
	public interface ISignatureInfo
	{
		List<ISignature> Signatures { get; }
		int ActiveSignature { get; }
		int ActiveParameter { get; }
	}
}
