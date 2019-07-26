using Microsoft.Extensions.Primitives;
using System;
using System.IO;

namespace TomPIT.UI
{
	public class ChangeToken : IChangeToken
	{
		private string _viewPath = string.Empty;

		public ChangeToken(string viewPath)
		{
			_viewPath = viewPath;
		}

		public bool ActiveChangeCallbacks => true;
		public bool HasChanged
		{
			get
			{
				if (Shell.HttpContext == null)
					return false;

				var kind = ViewInfo.ResolveViewKind(_viewPath);

				if (kind == ViewKind.View)
				{
					var path = _viewPath.Substring(7).Substring(0, _viewPath.Length - 14);

					return Instance.GetService<IViewService>().HasChanged(kind, path, ViewEngineBase.CreateActionContext(Shell.HttpContext));
				}
				else if (kind == ViewKind.Snippet)
				{
					var snippetKind = ViewInfo.ResolveSnippetKind(_viewPath);
					return Instance.GetService<IViewService>().HasSnippetChanged(snippetKind, Path.GetFileNameWithoutExtension(_viewPath), ViewEngineBase.CreateActionContext(Shell.HttpContext));
				}
				else if (kind == ViewKind.Report)
					return Instance.GetService<IViewService>().HasChanged(kind, _viewPath, null);
				else if (kind == ViewKind.MailTemplate)
				{
					var tokens = _viewPath.Split('/');
					var path = $"{tokens[tokens.Length - 2]}/{tokens[tokens.Length - 1]}";

					return Instance.GetService<IViewService>().HasChanged(kind, path, null);
				}
				else
					return Instance.GetService<IViewService>().HasChanged(kind, Path.GetFileNameWithoutExtension(_viewPath), null);
			}
		}

		public IDisposable RegisterChangeCallback(Action<object> callback, object state) => EmptyDisposable.Instance;
	}

	internal class EmptyDisposable : IDisposable
	{
		public static EmptyDisposable Instance { get; } = new EmptyDisposable();
		private EmptyDisposable() { }
		public void Dispose() { }
	}
}