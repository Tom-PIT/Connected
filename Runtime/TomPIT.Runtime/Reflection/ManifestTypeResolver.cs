using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Reflection.CodeAnalysis;

namespace TomPIT.Reflection
{
	internal class ManifestTypeResolver : TenantObject, IManifestTypeResolver, IDisposable
	{
		private static readonly List<string> ReservedPrimitiveTypes = new List<string>
		{
			typeof(Guid).FullName,
			typeof(TimeSpan).FullName
		};

		private Dictionary<string, IManifestTypeDescriptor> _cache;
		private bool _disposed;

		public ManifestTypeResolver(ITenant tenant, Guid microService, Guid component, Guid script) : base(tenant)
		{
			Component = component;
			Script = script;
			MicroService = microService;
		}

		private Guid MicroService { get; set; }
		private Guid Component { get; }
		private Guid Script { get; }
		private IScriptManifest Manifest { get; set; }
		private IText SourceCode { get; set; }
		private Dictionary<string, IManifestTypeDescriptor> Cache => _cache ??= new Dictionary<string, IManifestTypeDescriptor>();
		private Microsoft.CodeAnalysis.Compilation Compilation { get; set; }

		public IManifestTypeDescriptor Resolve(string name)
		{
			if (Manifest is null)
				Prepare();

			if (Cache.TryGetValue(name, out IManifestTypeDescriptor existing))
				return existing;

			return CreateType(name);
		}

		private void Prepare()
		{
			Manifest = Tenant.GetService<IDiscoveryService>().Manifests.SelectScript(MicroService, Component, Script);

			if (Manifest is null)
				return;

			if (Tenant.GetService<IDiscoveryService>().Configuration.Find(Component, Script) is not IText text)
				return;

			SourceCode = text;

			Compilation = Tenant.GetService<ICompilerService>().GetCompilation(text);
		}

		private IManifestTypeDescriptor CreateType(string name)
		{
			var symbols = Compilation.GetSymbolsWithName(name);

			if (!symbols.Any())
				return CreateFromDefault(name);

			foreach (var symbol in symbols)
			{
				if (symbol.Kind == SymbolKind.NamedType)
				{
					var type = symbol as INamedTypeSymbol;

					if (type is not null && string.Compare(type.ToDisplayString(), name, false) == 0)
						return CreateDescriptor(type);
				}
			}

			return null;
		}

		private IManifestTypeDescriptor CreateFromDefault(string name)
		{
			var tree = Compilation.SyntaxTrees.FirstOrDefault(f => string.Compare(f.FilePath, SourceCode.FileName, true) == 0);

			if (tree is null)
				return null;

			var model = Compilation.GetSemanticModel(tree);

			if (ProcessTree(tree, model, name) is IManifestTypeDescriptor result)
				return result;

			foreach(var other in Compilation.SyntaxTrees)
			{
				if (other == tree)
					continue;

				model = Compilation.GetSemanticModel(other);

				if (ProcessTree(other, model, name) is IManifestTypeDescriptor otherResult)
					return otherResult;
			}

			return null;
		}

		private IManifestTypeDescriptor ProcessTree(SyntaxTree tree, SemanticModel model, string name)
		{
			foreach (var node in tree.GetRoot().DescendantNodesAndSelf())
			{
				if (node.IsKind(SyntaxKind.PredefinedType) || node.IsKind(SyntaxKind.GenericName) || node.IsKind(SyntaxKind.QualifiedName) || node.IsKind(SyntaxKind.IdentifierName))
				{
					var typeInfo = model.GetTypeInfo(node);

					if (typeInfo.Type is not null && string.Compare(typeInfo.Type.ToDisplayString(), name, false) == 0)
						return CreateDescriptor(typeInfo.Type);
				}
			}

			return null;
		}

		private IManifestTypeDescriptor CreateDescriptor(ITypeSymbol symbol)
		{
			if (symbol == null)
				return null;

			if (Cache.TryGetValue(symbol.ToDisplayString(), out IManifestTypeDescriptor existing))
				return existing;

			var result = new ManifestTypeDescriptor
			{
				Name = symbol.ToDisplayString(),
				IsPrimitive = ResolvePrimitive(symbol),
			};

			Cache.TryAdd(symbol.ToDisplayString(), result);

			if (!result.IsPrimitive)
				ResolveMetaData(result, symbol);

			if (result.IsPrimitive)
				return result;

			ResolveTypeArguments(result, symbol);

			if (DiscoverMembers(result, symbol))
				ResolveMembers(result, symbol);
			
			return result;
		}

		private static bool DiscoverMembers(IManifestTypeDescriptor descriptor, ITypeSymbol symbol)
		{
			if (descriptor.IsArray)
				return false;

			if (symbol.AllInterfaces.Any(f => string.Compare(f.ToDisplayString(), typeof(IDictionary).FullName, false) == 0
				|| string.Compare(f.ToDisplayString(), typeof(ITuple).FullName, false) == 0))
				return false;

			return true;
		}

		private void ResolveMembers(IManifestTypeDescriptor descriptor, ITypeSymbol symbol)
		{
			var members = symbol.GetMembers();

			foreach (var member in members)
			{
				if (member.DeclaredAccessibility != Accessibility.Public || (member.Kind != SymbolKind.Field && member.Kind != SymbolKind.Property))
					continue;

				if (!member.GetAttributes().IsBrowsable())
					continue;

				if (member is IPropertySymbol property)
					descriptor.Members.TryAdd(member.Name, CreateDescriptor(property.Type));
				else if (member is IFieldSymbol field)
					descriptor.Members.TryAdd(member.Name, CreateDescriptor(field.Type));
				else
					throw new NotSupportedException();
			}
		}

		private void ResolveTypeArguments(IManifestTypeDescriptor descriptor, ITypeSymbol symbol)
		{
			if (symbol is IArrayTypeSymbol array)
				descriptor.TypeArguments.Add(array.ElementType.ToDisplayString(), CreateDescriptor(array.ElementType));
			else
			{
				if (symbol is not INamedTypeSymbol namedType || namedType.TypeArguments.IsDefaultOrEmpty)
					return;

				foreach (var argument in namedType.TypeArguments)
					descriptor.TypeArguments.Add(argument.Name, CreateDescriptor(argument));
			}
		}

		private static void ResolveMetaData(ManifestTypeDescriptor descriptor, ITypeSymbol symbol)
		{
			foreach (var i in symbol.AllInterfaces)
			{
				if (string.Compare(i.ToDisplayString(), typeof(IEnumerable).FullName, false) == 0)
					descriptor.IsArray = true;
				else if(string.Compare(i.ToDisplayString(), typeof(IDictionary).FullName, false) == 0)
				{
					descriptor.IsArray = false;
					break;
				}
			}
		}

		private static bool ResolvePrimitive(ITypeSymbol symbol)
		{
			if (symbol.SpecialType == SpecialType.System_Boolean
				|| symbol.SpecialType == SpecialType.System_Byte
				|| symbol.SpecialType == SpecialType.System_SByte
				|| symbol.SpecialType == SpecialType.System_Char
				|| symbol.SpecialType == SpecialType.System_DateTime
				|| symbol.SpecialType == SpecialType.System_Decimal
				|| symbol.SpecialType == SpecialType.System_Double
				|| symbol.SpecialType == SpecialType.System_Enum
				|| symbol.SpecialType == SpecialType.System_Int16
				|| symbol.SpecialType == SpecialType.System_Int32
				|| symbol.SpecialType == SpecialType.System_Int64
				|| symbol.SpecialType == SpecialType.System_Single
				|| symbol.SpecialType == SpecialType.System_String
				|| symbol.SpecialType == SpecialType.System_UInt16
				|| symbol.SpecialType == SpecialType.System_UInt32
				|| symbol.SpecialType == SpecialType.System_UInt64)
				return true;

			return ReservedPrimitiveTypes.Contains(symbol.ToDisplayString());
		}

		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Manifest = null;
					Cache.Clear();
				}

				_disposed = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
