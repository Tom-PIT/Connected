using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using TomPIT.Ide.TextServices.CSharp.Services.DecorationsProvider;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.Ide.TextServices.Services;

namespace TomPIT.Ide.TextServices.CSharp.Services
{
	internal class DeltaDecorationsService : CSharpEditorService, IDeltaDecorationsService
	{
		private List<IDeltaDecorationProvider> _providers;

		public DeltaDecorationsService(CSharpEditor editor) : base(editor)
		{
		}

		public ImmutableList<IDeltaDecoration> ProvideDecorations()
		{
			var model = Editor.SemanticModel;

			if (model == null)
				return ImmutableList<IDeltaDecoration>.Empty;

			var result = new List<IDeltaDecoration>();
			var args = new DeltaDecorationProviderArgs(Editor, model);

			foreach (var provider in Providers)
			{
				var results = provider.ProvideDecorations(args);

				if (results is not null && results.Any())
					result.AddRange(results);
			}

			return result.ToImmutableList();
		}

		private List<IDeltaDecorationProvider> Providers
		{
			get
			{
				if (_providers is null)
				{
					_providers = new List<IDeltaDecorationProvider>
					{
						new PartialClassProvider()
					};
				}

				return _providers;
			}
		}
	}
}
