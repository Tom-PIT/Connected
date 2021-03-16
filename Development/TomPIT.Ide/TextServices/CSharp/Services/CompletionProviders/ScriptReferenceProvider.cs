using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.Configuration;
using TomPIT.ComponentModel.Data;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.Reflection;

namespace TomPIT.Ide.TextServices.CSharp.Services.CompletionProviders
{
	internal class ScriptReferenceProvider : CompletionProvider
	{
		protected override List<ICompletionItem> OnProvideItems()
		{
			var result = new List<ICompletionItem>();

			var service = Microsoft.CodeAnalysis.Completion.CompletionService.GetService(Editor.Document);
			var span = service.GetDefaultCompletionListSpan(Editor.SourceText, Editor.Document.GetCaret(Arguments.Position));
			var trivia = Arguments.Model.SyntaxTree.GetRoot().FindTrivia(span.Start);

			if (trivia.IsKind(SyntaxKind.LoadDirectiveTrivia))
			{
				FillItems(result, Editor.Context.MicroService);

				var refs = Editor.Context.Tenant.GetService<IDiscoveryService>().MicroServices.References.Select(Editor.Context.MicroService.Token);

				foreach (var reference in refs.MicroServices)
				{
					var ms = Editor.Context.Tenant.GetService<IMicroServiceService>().Select(reference.MicroService);

					if (ms == null)
						continue;

					FillItems(result, ms);
				}
			}

			return result;
		}

		private void FillItems(List<ICompletionItem> items, IMicroService microService)
		{
			var msName = $"{microService.Name}/";
			var scripts = Editor.Context.Tenant.GetService<IComponentService>().QueryComponents(microService.Token, ComponentCategories.Script);

			foreach (var script in scripts)
				items.Add(CreateScriptItem($"{msName}{script.Name}"));

			var apis = Editor.Context.Tenant.GetService<IComponentService>().QueryConfigurations(microService.Token, ComponentCategories.Api);

			foreach (IConfiguration api in apis)
			{
				if (api is not IApiConfiguration configuration)
					continue;

				var apiName = api.ComponentName();

				foreach (var operation in configuration.Operations)
					items.Add(CreateScriptItem($"{msName}{apiName}/{operation.Name}"));
			}

			var models = Editor.Context.Tenant.GetService<IComponentService>().QueryConfigurations(microService.Token, ComponentCategories.Model);

			foreach (IConfiguration model in models)
			{
				if (model is not IModelConfiguration mc)
					continue;

				var modelName = mc.ComponentName();

				items.Add(CreateScriptItem($"{microService.Name}/{modelName}"));
			}

			var settings = Editor.Context.Tenant.GetService<IComponentService>().QueryConfigurations(microService.Token, ComponentCategories.Settings);

			foreach (IConfiguration setting in settings)
			{
				if (setting is not ISettingsConfiguration sc)
					continue;

				var modelName = sc.ComponentName();

				items.Add(CreateScriptItem($"{microService.Name}/{modelName}"));
			}
		}

		private static ICompletionItem CreateScriptItem(string name)
		{
			var result = new CompletionItem
			{
				FilterText = name,
				InsertText = $"\"{name}\"",
				Kind = CompletionItemKind.Reference,
				Label = name,
				SortText = name
			};

			result.CommitCharacters.AddRange(new List<string> { "\t", "\"", "\r" });

			return result;
		}

	}
}
