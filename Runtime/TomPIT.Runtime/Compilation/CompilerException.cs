using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.IoC;
using TomPIT.ComponentModel.UI;
using TomPIT.Exceptions;

namespace TomPIT.Compilation
{
	public class CompilerException : ScriptException
	{
		private string _message = string.Empty;

		public CompilerException(IViewConfiguration view, Exception inner) : base(view, inner)
		{
			_message = inner.Message;

			if (view == null)
				return;

			var ms = Tenant.GetService<IMicroServiceService>().Select(view.MicroService());

			Source = $"{view.ComponentName()}.cshtml";
			Path = $"{ms.Name}/{view.ComponentName()}.cshtml";
			MicroService = ms.Name;
			Component = view.Id;

			if (inner != null && inner.Data.Count > 0)
			{
				foreach (var data in inner.Data.Keys)
				{
					if (Data.Contains(data))
						Data[data] = inner.Data[data];
					else
						Data.Add(data, inner.Data[data]);
				}
			}
		}
		public CompilerException(IScriptDescriptor script, IText sourceCode)
		{
			var sb = new StringBuilder();

			foreach (var error in script.Errors)
			{
				if (error.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
					sb.AppendLine(error.Message);
			}

			var lastError = LastScriptError(script.Errors);

			if (lastError != null)
				ResolveComponent(sourceCode, lastError);
			else
			{
				if (script.Errors.Count > 0)
					Line = (script.Errors[^1].StartLine + 1).ToString();

				var ms = Tenant.GetService<IMicroServiceService>().Select(sourceCode.Configuration().MicroService());

				Source = sourceCode.FileName;
				Path = sourceCode.ResolvePath();
				MicroService = ms.Name;
				Component = sourceCode.Configuration().Component;
				Element = sourceCode.Id;
			}

			_message = sb.ToString();
		}

		public override string Message => _message;
		private void ResolveComponent(IText sourceCode, IDiagnostic diagnostic)
		{
			Path = diagnostic.SourcePath;
			Line = (diagnostic.StartLine + 1).ToString();

			var tokens = diagnostic.SourcePath.Split('/');

			if (tokens.Length == 1)
			{
				var ms = Tenant.GetService<IMicroServiceService>().Select(sourceCode.Configuration().MicroService());

				if (ms == null)
					return;

				MicroService = ms.Name;
				Component = sourceCode.Configuration().Component;
				Element = sourceCode.Id;

				return;
			}

			var microService = Tenant.GetService<IMicroServiceService>().Select(tokens[0]);

			if (microService == null)
				return;

			MicroService = microService.Name;

			var component = Tenant.GetService<IComponentService>().SelectComponentByNameSpace(microService.Token, ComponentCategories.NameSpacePublicScript, System.IO.Path.GetFileNameWithoutExtension(tokens[1]));

			if (component == null)
				return;

			Component = component.Token;

			var config = Tenant.GetService<IComponentService>().SelectConfiguration(component.Token);

			if (config == null)
				return;

			if (tokens.Length == 2)
			{
				Source = (config as IText).FileName;
				Element = component.Token;
			}
			else
			{
				if (config is IApiConfiguration api)
				{
					var operation = api.Operations.FirstOrDefault(f => string.Compare(f.Name, System.IO.Path.GetFileNameWithoutExtension(tokens[2]), true) == 0);

					if (operation != null)
					{
						Source = operation.FileName;
						Element = operation.Id;
					}
				}
				else if (config is IIoCContainerConfiguration ioc)
				{
					var operation = ioc.Operations.FirstOrDefault(f => string.Compare(f.Name, System.IO.Path.GetFileNameWithoutExtension(tokens[2]), true) == 0);

					if (operation != null)
					{
						Source = operation.FileName;
						Element = operation.Id;
					}
				}
			}
		}

		private IDiagnostic LastScriptError(List<IDiagnostic> items)
		{
			for (var i = items.Count - 1; i >= 0; i--)
			{
				var diagnostic = items[i];

				if (diagnostic.Severity != Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
					continue;

				if (!string.IsNullOrWhiteSpace(diagnostic.Source))
					return diagnostic;
			}

			return null;
		}
	}
}
