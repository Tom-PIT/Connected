using System.Collections.Generic;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.CSharp.Services.DecorationsProvider
{
	internal abstract class DeltaDecorationProvider : IDeltaDecorationProvider
	{
		public List<IDeltaDecoration> ProvideDecorations(DeltaDecorationProviderArgs e)
		{
			Arguments = e;

			return OnProvideDecorations();
		}

		protected virtual List<IDeltaDecoration> OnProvideDecorations()
		{
			return null;
		}

		protected DeltaDecorationProviderArgs Arguments { get; private set; }
		protected CSharpEditor Editor => Arguments.Editor as CSharpEditor;

	}
}
