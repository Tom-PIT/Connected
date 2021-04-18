using System.Collections.Generic;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.CSharp.Services.DecorationsProvider
{
	internal interface IDeltaDecorationProvider
	{
		List<IDeltaDecoration> ProvideDecorations(DeltaDecorationProviderArgs e);
	}
}
