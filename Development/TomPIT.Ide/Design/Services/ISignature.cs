using System.Collections.Generic;

namespace TomPIT.Design.Services
{
	public interface ISignature
	{
		string Label { get; }
		string Documentation { get; }
		List<ISignatureParameter> Parameters { get; }
	}
}
