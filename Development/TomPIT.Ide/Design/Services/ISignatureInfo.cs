using System.Collections.Generic;

namespace TomPIT.Design.Services
{
	public interface ISignatureInfo
	{
		List<ISignature> Signatures { get; }
		int ActiveSignature { get; }
		int ActiveParameter { get; }
	}
}
