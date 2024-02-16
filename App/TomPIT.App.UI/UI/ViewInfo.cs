using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using System;
using System.IO;
using System.Linq;
using System.Text;
using TomPIT.Annotations.Design;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Cdn;
using TomPIT.ComponentModel.Reports;
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

			var content = MiddlewareDescriptor.Current.Tenant.GetService<IViewCompilerService>().CompileView(MiddlewareDescriptor.Current.Tenant, config);
			var info = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectTextInfo(ViewComponent.MicroService, config.TextBlob, BlobTypes.SourceText);

			if (!string.IsNullOrWhiteSpace(content))
				_viewContent = Encoding.UTF8.GetBytes(content);

			LastModified = info is null ? DateTime.UtcNow : info.Modified;
		}

		private void LoadPartial()
		{
			var partial = MiddlewareDescriptor.Current.Tenant.GetService<IViewService>().SelectPartial(Path);

			Exists = partial != null;

			if (!Exists)
				return;

			ViewComponent = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectComponent(partial.Component);
			var info = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectTextInfo(partial.MicroService(), partial.TextBlob, BlobTypes.SourceText);

			LastModified = info is null ? DateTime.UtcNow : info.Modified;

			var content = MiddlewareDescriptor.Current.Tenant.GetService<IViewCompilerService>().CompileView(MiddlewareDescriptor.Current.Tenant, partial);

			if (!string.IsNullOrWhiteSpace(content))
				_viewContent = Encoding.UTF8.GetBytes(content);
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
			var content = MiddlewareDescriptor.Current.Tenant.GetService<IViewCompilerService>().CompileView(MiddlewareDescriptor.Current.Tenant, reportConfig);
			var info = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectTextInfo(report.MicroService, reportConfig.TextBlob, BlobTypes.SourceText);

			if (!string.IsNullOrWhiteSpace(content))
				_viewContent = Encoding.UTF8.GetBytes(content);

			LastModified = info is null ? DateTime.UtcNow : info.Modified;
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
				var info = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectTextInfo(config.MicroService(), view.TextBlob, BlobTypes.SourceText);

				LastModified = info is null ? DateTime.UtcNow : info.Modified;
			}

			var content = MiddlewareDescriptor.Current.Tenant.GetService<IViewCompilerService>().CompileView(MiddlewareDescriptor.Current.Tenant, view);

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
			var info = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectTextInfo(config.MicroService(), master.TextBlob, BlobTypes.SourceText);

			LastModified = info is null ? DateTime.UtcNow : info.Modified;

			var content = MiddlewareDescriptor.Current.Tenant.GetService<IViewCompilerService>().CompileView(MiddlewareDescriptor.Current.Tenant, master);

			if (!string.IsNullOrWhiteSpace(content))
				_viewContent = Encoding.UTF8.GetBytes(content);
		}
	}
}