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
		public bool HasChanged => Instance.GetService<IViewService>().HasChanged(ViewInfo.ResolveViewKind(_viewPath), Path.GetFileNameWithoutExtension(_viewPath));

		public IDisposable RegisterChangeCallback(Action<object> callback, object state) => EmptyDisposable.Instance;
	}

	internal class EmptyDisposable : IDisposable
	{
		public static EmptyDisposable Instance { get; } = new EmptyDisposable();
		private EmptyDisposable() { }
		public void Dispose() { }
	}
}