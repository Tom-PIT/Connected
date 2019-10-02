using System;
using System.IO;
using Microsoft.Extensions.Primitives;
using TomPIT.Middleware;

namespace TomPIT.App.UI
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
				var kind = ViewInfo.ResolveViewKind(_viewPath);

				if (kind == ViewKind.View)
				{
					var path = _viewPath.Substring(7).Substring(0, _viewPath.Length - 14);

					return MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().HasChanged(kind, path);
				}
				else if (kind == ViewKind.Snippet)
				{
					var snippetKind = ViewInfo.ResolveSnippetKind(_viewPath);
					return MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().HasSnippetChanged(snippetKind, Path.GetFileNameWithoutExtension(_viewPath));
				}
				else if (kind == ViewKind.Report)
					return MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().HasChanged(kind, _viewPath);
				else if (kind == ViewKind.MailTemplate)
				{
					var tokens = _viewPath.Split('/');
					var path = $"{tokens[tokens.Length - 2]}/{tokens[tokens.Length - 1]}";

					return MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().HasChanged(kind, path);
				}
				else
					return MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().HasChanged(kind, Path.GetFileNameWithoutExtension(_viewPath));
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