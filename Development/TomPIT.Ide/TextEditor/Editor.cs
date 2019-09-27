using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Host;
using TomPIT.ComponentModel;
using TomPIT.Ide.TextEditor.Languages;
using TomPIT.Middleware;

namespace TomPIT.Ide.TextEditor
{
	public abstract class Editor : MicroServiceObject, ITextEditor
	{
		private Lazy<Dictionary<Type, IWorkspaceService>> _services = new Lazy<Dictionary<Type, IWorkspaceService>>();

		protected Editor(IMicroServiceContext context) : base(context)
		{

		}

		public string Text { get; set; }
		public ITextModel Model { get; set; }
		public Type HostType { get; set; }

		public abstract Workspace Workspace { get; }
		public List<ICodeAction> ProvideCodeActions(IRange range, ICodeActionContext context)
		{
			return OnProvideCodeActions(range, context);
		}

		protected virtual List<ICodeAction> OnProvideCodeActions(IRange range, ICodeActionContext context)
		{
			return new List<ICodeAction>();
		}
		public void Dispose()
		{
			OnDispose();
		}

		protected virtual void OnDispose()
		{

		}

		public T GetService<T>() where T : IWorkspaceService
		{
			if (Services.ContainsKey(typeof(T)))
				return (T)Services[typeof(T)];

			return default;
		}

		protected Dictionary<Type, IWorkspaceService> Services => _services.Value;

		public virtual LanguageFeature Features => LanguageFeature.None;

		public IMicroService MicroService { get; set; }
	}
}
