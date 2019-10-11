using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.UI;
using TomPIT.Connectivity;
using TomPIT.Exceptions;

namespace TomPIT.Compilation
{
	public class CompilerException : ScriptException
	{
		private string _message = string.Empty;

		public CompilerException(ITenant tenant, IViewConfiguration view, Exception inner) : base(view, inner)
		{
			_message = inner.Message;

			if (view == null)
				return;

			var ms = tenant.GetService<IMicroServiceService>().Select(view.MicroService());

			Source = $"{view.ComponentName()}.cshtml";
			Path = $"{ms.Name}/{view.ComponentName()}.cshtml";
			MicroService = ms.Name;
			Component = view.Id;
		}
		public CompilerException(ITenant tenant, IScriptDescriptor script, IText sourceCode)
		{
			var sb = new StringBuilder();

			foreach (var error in script.Errors)
				sb.AppendLine(error.Message);

			var lastError = LastScriptError(script.Errors);

			if (lastError != null)
				ResolveComponent(tenant, sourceCode, lastError);
			else
			{
				if (script.Errors.Count > 0)
					Line = (script.Errors[^1].StartLine + 1).ToString();

				var ms = tenant.GetService<IMicroServiceService>().Select(sourceCode.Configuration().MicroService());

				Source = sourceCode.ScriptName(tenant);
				Path = sourceCode.ResolvePath(tenant);
				MicroService = ms.Name;
				Component = sourceCode.Configuration().Component;
				Element = sourceCode.Id;
			}

			_message = sb.ToString();
		}

		public override string Message => _message;
		private void ResolveComponent(ITenant tenant, IText sourceCode, IDiagnostic diagnostic)
		{
			Path = diagnostic.Source;
			Line = (diagnostic.StartLine + 1).ToString();

			var tokens = diagnostic.Source.Split('/');

			if (tokens.Length == 1)
			{
				var ms = tenant.GetService<IMicroServiceService>().Select(sourceCode.Configuration().MicroService());

				if (ms == null)
					return;

				MicroService = ms.Name;
				Component = sourceCode.Configuration().Component;
				Element = sourceCode.Id;

				return;
			}

			var microService = tenant.GetService<IMicroServiceService>().Select(tokens[0]);

			if (microService == null)
				return;

			MicroService = microService.Name;

			var component = tenant.GetService<IComponentService>().SelectComponentByNameSpace(microService.Token, ComponentCategories.NameSpacePublicScript, System.IO.Path.GetFileNameWithoutExtension(tokens[1]));

			if (component == null)
				return;

			Component = component.Token;

			var config = tenant.GetService<IComponentService>().SelectConfiguration(component.Token);

			if (config == null)
				return;

			if (tokens.Length == 2)
			{
				Source = (config as IText).ScriptName(tenant);
				Element = component.Token;
			}
			else
			{
				var operation = (config as IApiConfiguration).Operations.FirstOrDefault(f => string.Compare(f.Name, System.IO.Path.GetFileNameWithoutExtension(tokens[2]), true) == 0);

				if (operation != null)
				{
					Source = operation.ScriptName(tenant);
					Element = operation.Id;
				}

			}
		}

		private IDiagnostic LastScriptError(List<IDiagnostic> items)
		{
			for (var i = items.Count - 1; i >= 0; i--)
			{
				var diagnostic = items[i];

				if (!string.IsNullOrWhiteSpace(diagnostic.Source))
					return diagnostic;
			}

			return null;
		}
	}
}
