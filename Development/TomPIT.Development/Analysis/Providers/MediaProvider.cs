using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Ide.Analysis;
using TomPIT.Middleware;

namespace TomPIT.Development.Analysis.Providers
{
	internal class MediaProvider : ComponentAnalysisProvider
	{
		public MediaProvider(IMiddlewareContext context) : base(context)
		{

		}

		protected override string ComponentCategory => ComponentCategories.Media;
		protected override bool FullyQualified => true;
		protected override void ProvideComponentLiterals(CodeAnalysisArgs e, List<ICodeAnalysisResult> items, IComponent component)
		{
			if (!(Context.Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is IMediaResourcesConfiguration config))
				return;

			foreach (var i in config.Files)
				ProvideComponentLiterals(component.Name, i, items);

			foreach (var i in config.Folders)
				ProvideComponentLiterals(component.Name, i, items);
		}

		private void ProvideComponentLiterals(string path, IMediaResourceFile file, List<ICodeAnalysisResult> items)
		{
			if (string.IsNullOrWhiteSpace(file.FileName))
				return;

			var ms = Context.Tenant.GetService<IMicroServiceService>().Select(file.Configuration().MicroService());
			var text = $"{ms.Name}/{path}/{file.FileName}";

			items.Add(new CodeAnalysisResult(text, text, null));
		}

		private void ProvideComponentLiterals(string path, IMediaResourceFolder folder, List<ICodeAnalysisResult> items)
		{
			if (string.IsNullOrWhiteSpace(folder.Name))
				return;

			path = $"{path}/{folder.Name}";

			foreach (var i in folder.Files)
				ProvideComponentLiterals(path, i, items);

			foreach (var i in folder.Folders)
				ProvideComponentLiterals(path, i, items);
		}
	}
}
