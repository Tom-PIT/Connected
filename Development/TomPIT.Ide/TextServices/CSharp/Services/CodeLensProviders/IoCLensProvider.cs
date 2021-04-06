using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoC;
using TomPIT.Design.CodeAnalysis;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.IoC;
using TomPIT.Reflection;

namespace TomPIT.Ide.TextServices.CSharp.Services.CodeLensProviders
{
	internal class IoCLensProvider : CodeLensProvider
	{
		protected override List<ICodeLens> OnProvideCodeLens()
		{
			if (!IsIoCOperation())
				return null;

			var name = ParseIoCName();
			var references = Arguments.Editor.Context.Tenant.GetService<IDiscoveryService>().MicroServices.References.ReferencedBy(Arguments.Editor.Context.MicroService.Token, false);
			var result = ProcessReference(Arguments.Editor.Context.MicroService, name);

			foreach (var reference in references)
				result += ProcessReference(reference, name);

			if (result == 0)
				return null;

			var span = GetSpan();

			return new List<ICodeLens>
			{
				new CodeLens
				{
					Id = "1",
					Range = new Range
					{
						EndColumn = span.EndLinePosition.Character + 1,
						EndLineNumber = span.EndLinePosition.Line + 1,
						StartColumn = span.StartLinePosition.Character + 1,
						StartLineNumber = span.StartLinePosition.Line + 1
					},
					Command = new Command
					{
						Id = "1",
						Title = $"{result} endpoints(s)"
					}
				}
			};
		}

		private string ParseIoCName()
		{
			var ms = Arguments.Editor.Context.MicroService.Name;
			var component = Arguments.Editor.Script.Configuration().ComponentName();
			var fileName = Path.GetFileNameWithoutExtension(Arguments.Editor.Script.FileName);

			return $"{ms}/{component}/{fileName}";
		}

		private int ProcessReference(IMicroService microService, string identifier)
		{
			var configurations = Arguments.Editor.Context.Tenant.GetService<IComponentService>().QueryConfigurations(microService.Token, ComponentCategories.IoCEndpoint);
			var result = 0;

			foreach (var configuration in configurations)
			{
				if (configuration is IIoCEndpointConfiguration endpoint)
				{
					foreach (var i in endpoint.Endpoints)
					{
						if (i is IIoCEndpoint ep && string.Compare(ep.Container, identifier, true) == 0)
							result++;
					}
				}
			}

			return result;
		}


		private bool IsIoCOperation()
		{
			var root = Arguments.Model.SyntaxTree.GetRoot();

			if (root is null)
				return false;

			if (root.FindClass(Path.GetFileNameWithoutExtension(Arguments.Editor.Script.FileName)) is not ClassDeclarationSyntax declaration)
				return false;
			
			return declaration.LookupBaseType(Arguments.Model, typeof(IIoCOperationMiddleware).FullName) is not null;
		}

		private FileLinePositionSpan GetSpan()
		{
			var root = Arguments.Model.SyntaxTree.GetRoot();

			if (root is null)
				return new FileLinePositionSpan();

			if (root.FindClass(Path.GetFileNameWithoutExtension(Arguments.Editor.Script.FileName)) is not ClassDeclarationSyntax declaration)
				return new FileLinePositionSpan();

			return declaration.GetLocation().GetMappedLineSpan();
		}
	}
}