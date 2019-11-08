using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Primitives;
using TomPIT.Middleware;
using TomPIT.UI;

namespace TomPIT.App.UI
{
	public class ChangeToken : IChangeToken
	{
		private string _viewPath = string.Empty;
		internal static readonly string[] SystemViews = new string[] { "_ViewImports.cshtml", "_ViewStart.cshtml" };

		public ChangeToken(string viewPath)
		{
			_viewPath = viewPath;
		}

		public bool ActiveChangeCallbacks => true;
		public bool HasChanged
		{
			get
			{
				if (!_viewPath.StartsWith("/Views/Dynamic", StringComparison.OrdinalIgnoreCase))
					return false;

				var fileName = Path.GetFileName(_viewPath);

				if (SystemViews.FirstOrDefault(f => string.Compare(f, fileName, true) == 0) != null)
					return false;

				var kind = MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().ResolveViewKind(_viewPath);

				if (kind == ViewKind.View)
				{
					var path = _viewPath[0..^7].Substring(20);

					return MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().HasChanged(kind, path);
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
				{
					var path = _viewPath.Split('/');
					var partialPath = $"{path[^2]}/{path[^1]}";

					if (partialPath.EndsWith(".cshtml"))
						partialPath = partialPath[0..^7];

					return MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().HasChanged(kind, partialPath);
				}
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