using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TomPIT.Ide.TextServices.CSharp.Services.CodeLensProviders;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.Ide.TextServices.Services;

namespace TomPIT.Ide.TextServices.CSharp.Services
{
	internal class CodeLensService : CSharpEditorService, ICodeLensService
	{
		private List<ICodeLensProvider> _providers;

		public CodeLensService(CSharpEditor editor) : base(editor)
		{
		}

		public ImmutableList<ICodeLens> ProvideCodeLens()
		{
			var model = Editor.SemanticModel;

			if (model == null)
				return ImmutableList<ICodeLens>.Empty;

			var result = new List<ICodeLens>();
			var args = new CodeLensProviderArgs(Editor, model);

			foreach (var provider in Providers)
			{
				var results = provider.ProvideCodeLens(args);

				if (results is not null && results.Any())
					result.AddRange(results);
			}

			return result.ToImmutableList();
		}

		private List<ICodeLensProvider> Providers
		{
			get
			{
				if (_providers is null)
				{
					_providers = new List<ICodeLensProvider>
			 {
						new ApiInjectionLensProvider(),
						new IoCLensProvider()
			 };
				}

				return _providers;
			}
		}
	}
}
