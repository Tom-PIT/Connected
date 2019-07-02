using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;
using TomPIT.Services;

namespace TomPIT.Design.CodeAnalysis.Providers
{
	internal class MediaProvider : ComponentAnalysisProvider
	{
		public MediaProvider(IExecutionContext context) : base(context)
		{

		}

		protected override string ComponentCategory => "Media";
		protected override bool FullyQualified => true;
		protected override void ProvideComponentLiterals(CodeAnalysisArgs e, List<ICodeAnalysisResult> items, IComponent component)
		{
			if (!(Context.Connection().GetService<IComponentService>().SelectConfiguration(component.Token) is IMediaResources config))
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

			var ms = Context.Connection().GetService<IMicroServiceService>().Select(file.MicroService(Context.Connection()));
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
