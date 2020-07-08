using System.Collections.Generic;
using TomPIT.Ide.TextServices.CSharp.Services.DefinitionProviders;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.Ide.TextServices.Services;

namespace TomPIT.Ide.TextServices.CSharp.Services
{
	internal class DefinitionProviderService : CSharpEditorService, IDefinitionProviderService
	{
		private List<IDefinitionProvider> _providers = null;
		public DefinitionProviderService(CSharpEditor editor) : base(editor)
		{
		}

		public ILocation ProvideDefinition(IPosition position)
		{
			var args = new DefinitionProviderArgs(Editor, Editor.Document.GetSemanticModelAsync().Result, position);

			foreach (var provider in Providers)
			{
				var result = provider.ProvideDefinition(args);

				if (result != null)
					return result;
			}

			return null;
		}

		private List<IDefinitionProvider> Providers
		{
			get
			{
				if (_providers == null)
				{
					_providers = new List<IDefinitionProvider>
					{
						new DefaultDefinitionProvider(),
						new ReferenceProvider(),
						new AttributeProvider(),
					};
				}

				return _providers;
			}
		}
	}
}
