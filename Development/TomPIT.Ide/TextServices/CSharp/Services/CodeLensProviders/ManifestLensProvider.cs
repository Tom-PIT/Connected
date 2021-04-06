using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.Ide.TextServices.Languages;
using TomPIT.Reflection;

namespace TomPIT.Ide.TextServices.CSharp.Services.CodeLensProviders
{
	internal class ManifestLensProvider : CodeLensProvider
	{
		internal class ScriptManifestLocationComparer : IEqualityComparer<IScriptManifestSymbolLocation>
		{
			public bool Equals(IScriptManifestSymbolLocation x, IScriptManifestSymbolLocation y)
			{
				if (ReferenceEquals(x, y))
					return true;

				if (x is null || y is null)
					return false;

				return x.EndCharacter == y.EndCharacter
					&& x.EndLine == y.EndLine
					&& x.StartCharacter == y.StartCharacter
					&& x.StartLine == y.StartLine;
			}

			public int GetHashCode([DisallowNull] IScriptManifestSymbolLocation obj)
			{
				return GetHashCode();
			}
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

			var items = new Dictionary<IScriptManifestSymbolLocation, int>(new ScriptManifestLocationComparer());

			foreach (var type in manifest.DeclaredTypes)
				ProvideLenses(manifest, type, references, items, manifest.GetPointer(manifest.Address));
			
			foreach(var item in items)
			{
				result.Add(new CodeLens
				{
					Id = "1",
					Range = new Range
					{
						EndColumn = item.Key.EndCharacter+1,
						EndLineNumber = item.Key.EndLine+1,
						StartColumn = item.Key.StartCharacter+1,
						StartLineNumber = item.Key.StartLine+1
					},
					Command = new Command
					{
						Id="1",
						Title = $"{item.Value} reference(s)"
					}
				});
			}

			return result;
		}

		private void ProvideLenses(IScriptManifest manifest, IScriptManifestType type, IImmutableList<IScriptManifest> references, Dictionary<IScriptManifestSymbolLocation, int> items, IScriptManifestPointer address)
		{
			foreach(var reference in references)
			{
				foreach(var symbol in reference.SymbolReferences)
				{
					if (!reference.GetPointer(symbol.Key.Address).Equals(address))
						continue;

					if (items.ContainsKey(symbol.Key.Location))
						items[symbol.Key.Location]++;
					else
						items.Add(symbol.Key.Location, 1);
				}
			}
		}
	}
}
