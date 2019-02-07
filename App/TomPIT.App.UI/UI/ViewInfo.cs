using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Linq;
using System.Text;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.UI;
using TomPIT.Storage;

namespace TomPIT.UI
{
	internal enum ViewKind
	{
		Master = 1,
		View = 2,
		Partial = 3,
		Snippet = 4
	}

	internal class ViewInfo : IFileInfo
	{
		private byte[] _viewContent = null;

		public ViewInfo(string viewPath, ActionContext context)
		{
			if (string.Compare(System.IO.Path.GetFileNameWithoutExtension(viewPath), "_viewimports", true) == 0)
				return;

			ResolvePath(viewPath);
			Load(context);
		}

		private void ResolvePath(string viewPath)
		{
			var path = viewPath.Trim('/');
			FullPath = path;

			if (path.StartsWith("Views/Dynamic/Master"))
				Kind = ViewKind.Master;
			else if (path.StartsWith("Views/Dynamic/Partial"))
				Kind = ViewKind.Partial;
			else if (path.StartsWith("Views/Dynamic/Snippet"))
				Kind = ViewKind.Snippet;
			else
				Kind = ViewKind.View;

			if (Kind == ViewKind.View)
			{
				Path = viewPath.Substring(7);
				Path = Path.Substring(0, Path.Length - 7);
			}
			else
				Path = System.IO.Path.GetFileNameWithoutExtension(viewPath);
		}

		private string FullPath { get; set; }
		public ViewKind Kind { get; set; }
		public string Path { get; set; }
		public bool Exists { get; private set; }
		public bool IsDirectory => false;
		public DateTimeOffset LastModified { get; private set; } = DateTimeOffset.MinValue;
		public IComponent ViewComponent { get; set; }
		private IBlob Blob { get; set; }

		public long Length
		{
			get
			{
				using (var stream = new MemoryStream(_viewContent))
				{
					return stream.Length;
				}
			}
		}

		public string Name => ViewComponent.Name;
		public string PhysicalPath => null;

		public Stream CreateReadStream()
		{
			return new MemoryStream(_viewContent);
		}

		public IConfiguration GetConfiguration()
		{
			switch (Kind)
			{
				case ViewKind.Master:
					return Instance.GetService<IViewService>().SelectMaster(Path);
				case ViewKind.View:
					return Instance.GetService<IViewService>().Select(Path, null);
				case ViewKind.Partial:
					return Instance.GetService<IViewService>().SelectPartial(Path);
				default:
					throw new NotSupportedException();
			}
		}
		private void Load(ActionContext context)
		{
			switch (Kind)
			{
				case ViewKind.Master:
					LoadMaster();
					break;
				case ViewKind.View:
					LoadView(context);
					break;
				case ViewKind.Snippet:
					LoadSnippet();
					break;
				case ViewKind.Partial:
					LoadPartial();
					break;
				default:
					throw new NotSupportedException();
			}
		}

		private void LoadPartial()
		{
			var partial = Instance.GetService<IViewService>().SelectPartial(Path);

			Exists = partial != null;

			if (!Exists)
				return;

			ViewComponent = Instance.GetService<IComponentService>().SelectComponent(partial.TextBlob);
			Blob = Instance.GetService<IStorageService>().Select(partial.TextBlob);

			var pp = new PartialProcessor(partial, Instance.GetService<IViewService>().SelectContent(partial));

			pp.Compile();

			_viewContent = Encoding.UTF8.GetBytes(pp.Result);

			LastModified = Blob == null ? DateTime.UtcNow : Blob.Modified;
		}

		private void LoadView(ActionContext context)
		{
			var view = Instance.GetService<IViewService>().Select(Path, context);

			Exists = view != null;

			if (!Exists)
				return;

			var config = view.Configuration();
			var rendererAtt = config.GetType().FindAttribute<ViewRendererAttribute>();
			ViewComponent = Instance.GetService<IComponentService>().SelectComponent(config.Component);

			if (rendererAtt != null)
			{
				var renderer = (rendererAtt.Type ?? Types.GetType(rendererAtt.TypeName)).CreateInstance<IViewRenderer>();

				_viewContent = Encoding.UTF8.GetBytes(renderer.CreateContent(null, view));

				LastModified = ViewComponent.Modified;
			}
			else
			{
				Blob = Instance.GetService<IStorageService>().Select(view.TextBlob);

				var p = new ViewProcessor(view, Instance.GetService<IViewService>().SelectContent(view));

				p.Compile();

				_viewContent = Encoding.UTF8.GetBytes(p.Result);
			}

			LastModified = Blob == null ? DateTime.UtcNow : Blob.Modified;
		}

		private void LoadMaster()
		{
			var master = Instance.GetService<IViewService>().SelectMaster(Path);

			Exists = master != null;

			if (!Exists)
				return;

			var config = master.Configuration();
			ViewComponent = Instance.GetService<IComponentService>().SelectComponent(config.Component);
			Blob = Instance.GetService<IStorageService>().Select(master.TextBlob);

			var mp = new MasterProcessor(master, Instance.GetService<IViewService>().SelectContent(master));

			mp.Compile();

			_viewContent = Encoding.UTF8.GetBytes(mp.Result);

			LastModified = Blob == null ? DateTime.UtcNow : Blob.Modified;
		}

		private void LoadSnippet()
		{
			var tokens = FullPath.Split('/');

			Enum.TryParse(tokens[3], true, out ViewKind r);

			switch (r)
			{
				case ViewKind.Master:
					LoadMasterSnippet(tokens[4]);
					break;
				case ViewKind.View:
					var viewPath = string.Empty;

					for (int i = 4; i < tokens.Length; i++)
						viewPath += string.Format("{0}/", tokens[i]);

					LoadViewSnippet(viewPath.Trim('/'));
					break;
				case ViewKind.Partial:
					LoadPartialSnippet(tokens[4]);
					break;
				default:
					throw new NotSupportedException();
			}
		}

		private void LoadPartialSnippet(string name)
		{
			var vt = System.IO.Path.GetFileNameWithoutExtension(name);
			var view = System.IO.Path.GetFileNameWithoutExtension(vt);
			var snippet = System.IO.Path.GetExtension(vt).Trim('.');

			if (!(Instance.GetService<IViewService>().SelectPartial(view) is IPartialView c))
				throw new RuntimeException(SR.ErrPartialViewNotFound);

			LoadSnippetContent(c.Component, snippet, c.Snippets);
		}

		private void LoadMasterSnippet(string name)
		{
			var vt = System.IO.Path.GetFileNameWithoutExtension(name);
			var view = System.IO.Path.GetFileNameWithoutExtension(vt);
			var snippet = System.IO.Path.GetExtension(vt).Trim('.');

			var c = Instance.GetService<IViewService>().SelectMaster(view) as IMasterView;

			if (c == null)
				throw new RuntimeException(SR.ErrMasterViewNotFound);

			LoadSnippetContent(c.Component, snippet, c.Snippets);
		}

		private void LoadViewSnippet(string name)
		{
			var vt = System.IO.Path.GetFileNameWithoutExtension(name);
			var view = System.IO.Path.GetFileNameWithoutExtension(vt);
			var snippet = System.IO.Path.GetExtension(vt).Trim('.');
			var url = string.Empty;
			var tokens = name.Split('/');

			if (tokens.Length == 1)
				url = view;
			else
			{
				for (int i = 0; i < tokens.Length - 1; i++)
					url += string.Format("{0}/", tokens[i]);

				url += string.Format("{0}/", view);

				url = url.Trim('/');
			}

			if (!(Instance.GetService<IViewService>().Select(url, null) is IView c))
				throw new RuntimeException(SR.ErrComponentNotFound);

			LoadSnippetContent(c.Component, snippet, c.Snippets);
		}

		private void LoadSnippetContent(Guid configuration, string snippet, ListItems<ISnippet> snippets)
		{
			if (!(snippets.FirstOrDefault(f => f is ISnippet && string.Compare(((ISnippet)f).Name, snippet, true) == 0) is ISnippet s))
				throw new RuntimeException(SR.ErrSnippetNotFound);

			Exists = true;

			ViewComponent = Instance.GetService<IComponentService>().SelectComponent(configuration);
			Blob = Instance.GetService<IStorageService>().Select(s.TextBlob);

			var sp = new SnippetProcessor(s, Instance.GetService<IComponentService>().SelectText(ViewComponent.MicroService, s));

			sp.Compile();

			_viewContent = Encoding.UTF8.GetBytes(sp.Result);

			LastModified = Blob == null ? DateTime.UtcNow : Blob.Modified;

		}
	}
}