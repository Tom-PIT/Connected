using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.ComponentModel;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.Reflection;

namespace TomPIT.Ide.TextServices.CSharp.Services.CodeLensProviders
{
	internal class ManifestLensProvider : CodeLensProvider
	{
		private class ManifestSymbolDescriptor
		{
			public int StartLine { get; set; }
			public int StartColumn { get; set; }
			public int EndLine { get; set; }
			public int EndColumn { get; set; }
			public int Count { get; set; }
		}

		protected override List<ICodeLens> OnProvideCodeLens()
		{
			var result = new List<ICodeLens>();

			var ms = Arguments.Editor.Script.Configuration().MicroService();
			var component = Arguments.Editor.Script.Configuration().Component;
			var manifest = Arguments.Editor.Context.Tenant.GetService<IDiscoveryService>().Manifests.SelectScript(ms, component, Arguments.Editor.Script.Id);

			if (manifest is null)
				return result;

			var references = Arguments.Editor.Context.Tenant.GetService<IDiscoveryService>().Manifests.QueryReferences(manifest);

			if (references.IsEmpty())
				return result;

			var items = new Dictionary<string, ManifestSymbolDescriptor>();

			ProvideLenses(references.Add(manifest), items, manifest.GetPointer(manifest.Address));

			foreach (var item in items)
			{
				result.Add(new CodeLens
				{
					Id = "1",
					Range = new Range
					{
						EndColumn = item.Value.EndColumn + 1,
						EndLineNumber = item.Value.EndLine + 1,
						StartColumn = item.Value.StartColumn + 1,
						StartLineNumber = item.Value.StartLine + 1
					},
					Command = new Command
					{
						Id = "1",
						Title = $"{item.Value.Count} reference(s)"
					}
				});
			}

			return result;
		}

		private void ProvideLenses(IImmutableList<IScriptManifest> references, Dictionary<string, ManifestSymbolDescriptor> items, IScriptManifestPointer address)
		{
			foreach (var reference in references)
			{
				foreach (var symbol in reference.SymbolReferences)
				{
					if (!reference.GetPointer(symbol.Key.Address).Equals(address))
						continue;

					if (items.ContainsKey(symbol.Key.Identifier))
						items[symbol.Key.Identifier].Count+= symbol.Value.Count;
					else
					{
						var identifierStack = new Queue<string>(symbol.Key.Identifier.Split('.'));

						var target = FindSymbol(Arguments.Model.SyntaxTree.GetRoot(), identifierStack);

						if (target is null)
							continue;

						target.Count = symbol.Value.Count;

						items.Add(symbol.Key.Identifier, target);
					}
				}
			}
		}

		private ManifestSymbolDescriptor FindSymbol(SyntaxNode node, Queue<string> stack)
		{
			var currentIdentifier = stack.Dequeue();

			foreach (var child in node.DescendantNodes())
			{
				if (child is MemberDeclarationSyntax member)
				{
					var name = ResolveName(member);

					if (string.IsNullOrWhiteSpace(name))
						continue;

					if (string.Compare(name, currentIdentifier, false) == 0)
					{
						if (stack.IsEmpty())
						{
							var loc = child.GetLocation().GetLineSpan();

							return new ManifestSymbolDescriptor
							{
								StartLine = loc.StartLinePosition.Line,
								EndColumn = loc.EndLinePosition.Character,
								EndLine = loc.EndLinePosition.Line,
								StartColumn = loc.StartLinePosition.Character
							};
						}

						return FindSymbol(child, stack);
					}
				}
			}

			return null;
		}

		private string ResolveName(MemberDeclarationSyntax syntax)
		{
			if (syntax is BaseTypeDeclarationSyntax bt)
				return bt.Identifier.ToString();
			else if (syntax is PropertyDeclarationSyntax pd)
				return pd.Identifier.ToString();
			else if (syntax is FieldDeclarationSyntax fd)
				return fd.Declaration.Variables.First().Identifier.ToString();
			else
				return null;
		}
		private ManifestSymbolDescriptor FindSymbol()
		{
			return null;
		}
	}
}
