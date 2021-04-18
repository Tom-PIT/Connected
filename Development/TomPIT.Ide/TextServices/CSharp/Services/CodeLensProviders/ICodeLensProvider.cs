using System.Collections.Generic;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.CSharp.Services.CodeLensProviders
{
	internal interface ICodeLensProvider
	{
		List<ICodeLens> ProvideCodeLens(CodeLensProviderArgs e);
	}
}
