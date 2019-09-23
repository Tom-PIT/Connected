using System;
using Microsoft.CodeAnalysis.Host;
using TomPIT.ComponentModel;
using TomPIT.Middleware;

namespace TomPIT.Ide.TextEditor
{
	public interface ITextEditor : IMiddlewareComponent, IDisposable
	{
		string Text { get; set; }
		ITextModel Model { get; set; }
		Type HostType { get; set; }
		IMicroService MicroService { get; set; }

		T GetService<T>() where T : IWorkspaceService;

		LanguageFeature Features { get; }
	}
}
