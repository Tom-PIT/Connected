using Microsoft.CodeAnalysis.Host;
using TomPIT.Ide.TextServices.Languages;

namespace TomPIT.Ide.TextServices.Services
{
	public interface ISignatureHelpService : IWorkspaceService
	{
		ISignatureHelp ProvideSignatureHelp(IPosition position, ISignatureHelpContext context);
	}
}
