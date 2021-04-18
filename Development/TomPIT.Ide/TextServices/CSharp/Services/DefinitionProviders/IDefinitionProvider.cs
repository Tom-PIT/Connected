using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.CSharp.Services.DefinitionProviders
{
	internal interface IDefinitionProvider
	{
		ILocation ProvideDefinition(DefinitionProviderArgs e);
	}
}
