using System.Collections.Generic;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.CSharp.Services.CodeLensProviders
{
	internal abstract class CodeLensProvider : ICodeLensProvider
	{
		public List<ICodeLens> ProvideCodeLens(CodeLensProviderArgs e)
		{
			Arguments = e;

			return OnProvideCodeLens();
		}

		protected virtual List<ICodeLens> OnProvideCodeLens()
		{
			return null;
		}

		protected CodeLensProviderArgs Arguments { get; private set; }
		protected CSharpEditor Editor => Arguments.Editor as CSharpEditor;

	}
}
