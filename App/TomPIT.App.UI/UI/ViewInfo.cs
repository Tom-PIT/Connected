using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.ComponentModel.Reports;
using TomPIT.ComponentModel.UI;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;
using TomPIT.Storage;
using TomPIT.UI;

namespace TomPIT.App.UI
{
	internal class ViewInfo : IFileInfo
	{
		private byte[] _viewContent = null;

		public ViewInfo(string viewPath, ActionContext action)
		{
			if (ChangeToken.SystemViews.FirstOrDefault(f => string.Compare(f, System.IO.Path.GetFileName(viewPath), true) == 0) != null)
				return;

			ResolvePath(viewPath);
			Load(action);
		}

		private void ResolvePath(string viewPath)
		{
			FullPath = viewPath.Trim('/');

			Kind = MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().ResolveViewKind(viewPath);

			if (Kind == ViewKind.View)
			{
				Path = viewPath.Substring(20);
				Path = Path[0..^7];
			}
			else if (Kind == ViewKind.Report)
			{
				var tokens = viewPath.Split('/');

				Path = $"{tokens[^2]}/{tokens[^1].Split('.')[0]}";
			}
			else
			{
				var tokens = FullPath.Split('/');

				Path = $"{tokens[^2]}/{System.IO.Path.GetFileNameWithoutExtension(tokens[^1])}";
			}
		}

		public static ViewKind ResolveSnippetKind(string url)
		{
			var tokens = url.Trim('/').Split('/');

			if (Enum.TryParse(tokens[3], true, out ViewKind r))
				return r;

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

				using var stream = new MemoryStream(_viewContent);

				return stream.Length;
			}
		}

		public string Name => ViewComponent.Name;
		public string PhysicalPath => null;

		public Stream CreateReadStream()
		{
			return new MemoryStream(_viewContent ?? Array.Empty<byte>());
		}

		public IConfiguration GetConfiguration()
		{
			return Kind switch
			{
				ViewKind.Master => MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().SelectMaster(Path),
				ViewKind.View => MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().Select(Path, null),
				ViewKind.Partial => MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().SelectPartial(Path),
				_ => throw new NotSupportedException(),
			};
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

			ViewComponent = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectComponent(new Guid(tokens[0]), "MailTemplate", System.IO.Path.GetFileNameWithoutExtension(tokens[1]));

			Exists = ViewComponent != null && string.Compare(ViewComponent.Category, "MailTemplate", true) == 0;

			if (!Exists)
				return;

			if (!(MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(ViewComponent.Token) is IMailTemplateConfiguration config))
				return;

			var content = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CompileView(MiddlewareDescriptor.Current.Tenant, config);

			if (!string.IsNullOrWhiteSpace(content))
				_viewContent = Encoding.UTF8.GetBytes(content);

			LastModified = Blob == null ? DateTime.UtcNow : Blob.Modified;
		}

		private void LoadPartial()
		{
			var partial = MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().SelectPartial(Path);

			Exists = partial != null;

			if (!Exists)
				return;

			ViewComponent = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectComponent(partial.Component);
			Blob = MiddlewareDescriptor.Current.Tenant.GetService<IStorageService>().Select(partial.TextBlob);

			var content = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CompileView(MiddlewareDescriptor.Current.Tenant, partial);

			if (!string.IsNullOrWhiteSpace(content))
				_viewContent = Encoding.UTF8.GetBytes(content);

			LastModified = Blob == null ? DateTime.UtcNow : Blob.Modified;
		}

		private void LoadReport()
		{
			var tokens = Path.Split('/');
			var ms = MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(tokens[0]);

			if (ms == null)
				return;

			var report = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectComponent(ms.Token, "Report", tokens[1]);

			Exists = report != null;

			if (!Exists)
				return;

			ViewComponent = report;
			var reportConfig = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(report.Token) as IReportConfiguration;

			var content = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CompileView(MiddlewareDescriptor.Current.Tenant, reportConfig);

			if (!string.IsNullOrWhiteSpace(content))
				_viewContent = Encoding.UTF8.GetBytes(content);

			LastModified = Blob == null ? DateTime.UtcNow : Blob.Modified;
		}

		private void LoadView(ActionContext context)
		{
			if (MiddlewareDescriptor.Current.Tenant == null)
				return;

			var view = MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().Select(Path, context);

			Exists = view != null;

			if (!Exists)
				return;

			var config = view.Configuration();
			var rendererAtt = config.GetType().FindAttribute<ViewRendererAttribute>();
			ViewComponent = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectComponent(config.Component);

			if (rendererAtt != null)
				LastModified = ViewComponent.Modified;
			else
			{
				Blob = MiddlewareDescriptor.Current.Tenant.GetService<IStorageService>().Select(view.TextBlob);
				LastModified = Blob == null ? DateTime.UtcNow : Blob.Modified;
			}

			var content = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CompileView(MiddlewareDescriptor.Current.Tenant, view);

			if (!string.IsNullOrWhiteSpace(content))
				_viewContent = Encoding.UTF8.GetBytes(content);
		}

		private void LoadMaster()
		{
			var master = MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().SelectMaster(Path);

			Exists = master != null;

			if (!Exists)
				return;

			var config = master.Configuration();
			ViewComponent = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectComponent(config.Component);
			Blob = MiddlewareDescriptor.Current.Tenant.GetService<IStorageService>().Select(master.TextBlob);

			var content = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CompileView(MiddlewareDescriptor.Current.Tenant, master);

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

			if (!(MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().SelectPartial(view) is IPartialViewConfiguration c))
				throw new RuntimeException(SR.ErrPartialViewNotFound);

			LoadSnippetContent(c.Component, snippet, c.Snippets);
		}

		private void LoadMasterSnippet(string name)
		{
			var vt = System.IO.Path.GetFileNameWithoutExtension(name);
			var view = System.IO.Path.GetFileNameWithoutExtension(vt);
			var snippet = System.IO.Path.GetExtension(vt).Trim('.');

			if (!(MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().SelectMaster(view) is IMasterViewConfiguration c))
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

			if (!(MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().Select(url, null) is IViewConfiguration c))
				throw new RuntimeException(SR.ErrComponentNotFound);

			LoadSnippetContent(c.Component, snippet, c.Snippets);
		}

		private void LoadSnippetContent(Guid configuration, string snippet, ListItems<ISnippet> snippets)
		{
			if (!(snippets.FirstOrDefault(f => f is ISnippet && string.Compare(((ISnippet)f).Name, snippet, true) == 0) is ISnippet s))
				throw new RuntimeException(SR.ErrSnippetNotFound);

			Exists = true;

			ViewComponent = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectComponent(configuration);
			Blob = MiddlewareDescriptor.Current.Tenant.GetService<IStorageService>().Select(s.TextBlob);

			var content = MiddlewareDescriptor.Current.Tenant.GetService<ICompilerService>().CompileView(MiddlewareDescriptor.Current.Tenant, s);

			if (!string.IsNullOrWhiteSpace(content))
				_viewContent = Encoding.UTF8.GetBytes(content);

			LastModified = Blob == null ? DateTime.UtcNow : Blob.Modified;
		}
	}
}