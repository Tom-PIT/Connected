using System;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Text;
using TomPIT.Middleware;

namespace TomPIT.Ide.TextServices
{
	public interface ITextEditor : IMicroServiceObject, IDisposable
	{
		string Text { get; set; }
		ITextModel Model { get; set; }
		Type HostType { get; set; }

		T GetService<T>() where T : IWorkspaceService;

		LanguageFeature Features { get; }

		int GetCaret(IPosition position);
		int GetMappedCaret(IPosition position);
		IPosition GetMappedPosition(IPosition position);
		TextSpan GetMappedSpan(IPosition position);
	}
}
