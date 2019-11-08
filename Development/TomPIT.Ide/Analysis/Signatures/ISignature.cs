using System.Collections.Generic;

namespace TomPIT.Ide.Analysis.Signatures
{
	public interface ISignature
	{
		string Label { get; }
		string Documentation { get; }
		List<ISignatureParameter> Parameters { get; }
	}
}
