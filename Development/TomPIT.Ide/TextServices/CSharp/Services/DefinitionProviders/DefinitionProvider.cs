using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.CSharp.Services.DefinitionProviders
{
	public class DefinitionProvider : IDefinitionProvider
	{
		public ILocation ProvideDefinition(DefinitionProviderArgs e)
		{
			return OnProvideDefinition(e);
		}

		protected virtual ILocation OnProvideDefinition(DefinitionProviderArgs e)
		{
			return null;
		}
	}
}
