using TomPIT.ComponentModel;
using TomPIT.Ide.TextServices;
using TomPIT.Ide.TextServices.CSharp.Services.DefinitionProviders;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Development.TextEditor.CSharp.Services.DefinitionProviders
{
	internal abstract class ComponentDefinitionProvider : DefinitionProvider
	{
		protected abstract string ComponentCategory { get; }

		protected override ILocation OnProvideDefinition(DefinitionProviderArgs e)
		{
			var caret = e.Editor.Document.GetCaret(e.Position);
			var nodeToken = e.Model.SyntaxTree.GetRoot().FindToken(caret);

			var tokens = nodeToken.ValueText.Split('/');
			var ms = e.Editor.Context.Tenant.GetService<IMicroServiceService>().Select(tokens[0]);
			var component = e.Editor.Context.Tenant.GetService<IComponentService>().SelectComponent(ms.Token, ComponentCategory, tokens[1]);

			return new Ide.TextServices.Languages.Location
			{
				Range = new Range
				{
					EndColumn = 1,
					EndLineNumber = 1,
					StartColumn = 1,
					StartLineNumber = 1
				},
				Uri = $"inmemory://{ms.Name}/{ComponentCategory}/{component.Name}/{component.Token.ToString()}"
			};
		}
	}
}
