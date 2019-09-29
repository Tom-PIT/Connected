using System;
using Microsoft.CodeAnalysis.Host;
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
	}
}
