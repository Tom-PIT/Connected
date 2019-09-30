using System.Collections.Generic;

namespace TomPIT.Ide.TextServices.Languages
{
	public interface ISignatureInformation
	{
		string Documentation { get; }
		string Label { get; }
		List<IParameterInformation> Parameters { get; }
	}
}
