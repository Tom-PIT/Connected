using System;
using Microsoft.CodeAnalysis;
using TomPIT.Compilation;
using TomPIT.ComponentModel;

namespace TomPIT.Ide.TextServices.CSharp.Services.DefinitionProviders
{
	public class DefinitionProviderArgs : EventArgs
	{
		public DefinitionProviderArgs(CSharpEditor editor, SemanticModel model, IPosition position)
		{
			Editor = editor;
			Model = model;
			Position = position;
		}

		public CSharpEditor Editor { get; }
		public SemanticModel Model { get; }
		public IPosition Position { get; }

		public string ResolveModel(string path)
		{
			var result = string.IsNullOrWhiteSpace(path)
				? Editor.Script
				: Editor.Context.Tenant.GetService<ICompilerService>().ResolveText(Editor.Context.MicroService.Token, path);

			if (result == null)
				return null;

			var component = Editor.Context.Tenant.GetService<IComponentService>().SelectComponent(result.Configuration().Component);
			var ms = Editor.Context.Tenant.GetService<IMicroServiceService>().Select(component.MicroService);

			return $"inmemory://{ms.Name}/{component.Category}/{component.Name}/{result.Id.ToString()}";
		}
	}
}
