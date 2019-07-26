using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Linq;
using System.Text;
using TomPIT.Annotations;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.ComponentModel.Reports;
using TomPIT.ComponentModel.UI;
using TomPIT.Services;
using TomPIT.Storage;

namespace TomPIT.UI
{
	internal enum ViewKind
	{
		Master = 1,
		View = 2,
		Partial = 3,
		Snippet = 4,
		MailTemplate = 5,
		Report = 6
	}

	internal class ViewInfo : IFileInfo
	{
		private byte[] _viewContent = null;

		public ViewInfo(string viewPath, ActionContext action)
		{
			if (string.Compare(System.IO.Path.GetFileNameWithoutExtension(viewPath), "_viewimports", true) == 0)
				return;

			ResolvePath(viewPath);
			Load(action);
		}

		private void ResolvePath(string viewPath)
		{
			FullPath = viewPath.Trim('/');

			Kind = ResolveViewKind(viewPath);

			if (Kind == ViewKind.View)
			{
				Path = viewPath.Substring(7);
				Path = Path.Substring(0, Path.Length - 7);
			}
			else if(Kind == ViewKind.Report)
			{
				var tokens = viewPath.Split('/');

				Path = $"{tokens[tokens.Length - 2]}/{tokens[tokens.Length - 1].Split('.')[0]}";
			}
			else if (Kind == ViewKind.MailTemplate)
			{
				var tokens = FullPath.Split('/');

				Path = $"{tokens[tokens.Length-2]}/{tokens[tokens.Length-1]}";
			}
			else
				Path = System.IO.Path.GetFileNameWithoutExtension(viewPath);
		}

		public static ViewKind ResolveSnippetKind(string url)
		{
			var tokens = url.Trim('/').Split('/');

			if (Enum.TryParse(tokens[3], true, out ViewKind r))
				return r;

			return ViewKind.View;
		}

		public static ViewKind ResolveViewKind(string viewPath)
		{
			var path = viewPath.Trim('/');

			if (path.StartsWith("Views/Dynamic/Master"))
				return ViewKind.Master;
			else if (path.StartsWith("Views/Dynamic/Partial"))
				return ViewKind.Partial;
			else if (path.StartsWith("Views/Dynamic/Snippet"))
				return ViewKind.Snippet;
			else if (path.StartsWith("Views/Dynamic/MailTemplate"))
				return ViewKind.MailTemplate;
			else if (path.StartsWith("Views/Dynamic/Report"))
				return ViewKind.Report;
			else
				return ViewKind.View;
		}

		private string FullPath { get; set; }
		public ViewKind Kind { get; set; }
		public string Path { get; set; }
		public bool Exists { get; private set; }
		public bool IsDirectory => false;
		public DateTimeOffset LastModified { get; private set; } = DateTimeOffset.MinValue;
		public IComponent ViewComponent { get; private set; }
		private IBlob Blob { get; set; }

		public long Length
		{
			get
			{
				if (_viewContent == null)
					return 0;

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
			return new MemoryStream(_viewContent == null ? new byte[0] : _viewContent);
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
				case ViewKind.MailTemplate:
					LoadMailTemplate();
					break;
				case ViewKind.Report:
					LoadReport();
					break;
				default:
					throw new NotSupportedException();
			}
		}

		private void LoadMailTemplate()
		{
			var tokens = Path.Split('/');

			ViewComponent = Instance.GetService<IComponentService>().SelectComponent(tokens[0].AsGuid(), "MailTemplate", System.IO.Path.GetFileNameWithoutExtension( tokens[1]));

			Exists = ViewComponent != null && string.Compare(ViewComponent.Category, "MailTemplate", true) == 0;

			if (!Exists)
				return;

			if (!(Instance.GetService<IComponentService>().SelectConfiguration(ViewComponent.Token) is IMailTemplate config))
				return;

			var content = Instance.GetService<ICompilerService>().CompileView(Instance.Connection, config);

			if (!string.IsNullOrWhiteSpace(content))
				_viewContent = Encoding.UTF8.GetBytes(content);

			LastModified = Blob == null ? DateTime.UtcNow : Blob.Modified;
		}

		private void LoadPartial()
		{
			var partial = Instance.GetService<IViewService>().SelectPartial(Path);

			Exists = partial != null;

			if (!Exists)
				return;

			ViewComponent = Instance.GetService<IComponentService>().SelectComponent(partial.TextBlob);
			Blob = Instance.GetService<IStorageService>().Select(partial.TextBlob);

			var content = Instance.GetService<ICompilerService>().CompileView(Instance.Connection, partial);

			if (!string.IsNullOrWhiteSpace(content))
				_viewContent = Encoding.UTF8.GetBytes(content);

			LastModified = Blob == null ? DateTime.UtcNow : Blob.Modified;
		}

		private void LoadReport()
		{
			var tokens = Path.Split('/');
			var ms = Instance.GetService<IMicroServiceService>().Select(tokens[0]);

			if (ms == null)
				return;

			var report = Instance.GetService<IComponentService>().SelectComponent(ms.Token, "Report", tokens[1]);

			Exists = report != null;

			if (!Exists)
				return;

			ViewComponent = report;
			var reportConfig = Instance.GetService<IComponentService>().SelectConfiguration(report.Token) as IReport;

			var content = Instance.GetService<ICompilerService>().CompileView(Instance.Connection, reportConfig);

			if (!string.IsNullOrWhiteSpace(content))
				_viewContent = Encoding.UTF8.GetBytes(content);

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
				LastModified = ViewComponent.Modified;
			else
			{
				Blob = Instance.GetService<IStorageService>().Select(view.TextBlob);
				LastModified = Blob == null ? DateTime.UtcNow : Blob.Modified;
			}

			var content = Instance.GetService<ICompilerService>().CompileView(Instance.Connection, view);

			if (!string.IsNullOrWhiteSpace(content))
				_viewContent = Encoding.UTF8.GetBytes(content);
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

			var content = Instance.GetService<ICompilerService>().CompileView(Instance.Connection, master);

			if (!string.IsNullOrWhiteSpace(content))
				_viewContent = Encoding.UTF8.GetBytes(content);

			LastModified = Blob == null ? DateTime.UtcNow : Blob.Modified;
		}

		private void LoadSnippet()
		{
			var tokens = FullPath.Split('/');

			switch (ResolveSnippetKind(FullPath))
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

			var content = Instance.GetService<ICompilerService>().CompileView(Instance.Connection, s);

			if (!string.IsNullOrWhiteSpace(content))
				_viewContent = Encoding.UTF8.GetBytes(content);

			LastModified = Blob == null ? DateTime.UtcNow : Blob.Modified;

		}
	}
}