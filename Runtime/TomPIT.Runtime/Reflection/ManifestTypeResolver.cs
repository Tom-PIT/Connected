using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;

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
		private SemanticModel Model { get; set; }
		private ClassDeclarationSyntax ClassDeclaration { get; set; }
		private Dictionary<string, IManifestTypeDescriptor> Cache => _cache ??= new Dictionary<string, IManifestTypeDescriptor>();
		public IManifestTypeDescriptor Resolve(string name)
		{
			if (Manifest is null)
				Prepare();

			if (ClassDeclaration is null)
				return null;

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
			var compilation = Tenant.GetService<ICompilerService>().GetCompilation(text);

			if (compilation is null)
				return;

			var tree = compilation.SyntaxTrees.FirstOrDefault(f => string.Compare(f.FilePath, text.FileName, true) == 0);
			var root = tree.GetCompilationUnitRoot();
			Model = compilation.GetSemanticModel(tree);

			if (root.Members.First(f => f is ClassDeclarationSyntax c && string.Compare(c.Identifier.ValueText, Path.GetFileNameWithoutExtension(SourceCode.FileName), false) == 0) is not ClassDeclarationSyntax declaration)
				return;

			ClassDeclaration = declaration;
		}

		private IManifestTypeDescriptor CreateType(string name)
		{
			foreach (var n in ClassDeclaration.DescendantNodesAndSelf())
			{
				if (n.IsKind(SyntaxKind.PredefinedType) || n.IsKind(SyntaxKind.GenericName) || n.IsKind(SyntaxKind.QualifiedName) || n.IsKind(SyntaxKind.IdentifierName))
				{
					var typeInfo = Model.GetTypeInfo(n);

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
				IsTuple = symbol.IsTupleType
			};

			Cache.TryAdd(symbol.ToDisplayString(), result);

			ResolveMetaData(result, symbol);

			if (result.IsPrimitive)
				return result;

			ResolveTypeArguments(result, symbol);

			if(!result.IsArray && !result.IsDictionary)
				ResolveMembers(result, symbol);

			return result;
		}

		private void ResolveMembers(IManifestTypeDescriptor descriptor, ITypeSymbol symbol)
		{
			var members = symbol.GetMembers();

			foreach (var member in members)
			{
				if (member.DeclaredAccessibility != Accessibility.Public || member.Kind != SymbolKind.Field || member.Kind != SymbolKind.Property)
					continue;

				descriptor.Members.TryAdd(member.Name, CreateDescriptor(member.ContainingType));
			}
		}

		private void ResolveTypeArguments(IManifestTypeDescriptor descriptor, ITypeSymbol symbol)
		{
			if (symbol is not INamedTypeSymbol namedType || namedType.TypeArguments.IsDefaultOrEmpty)
				return;

			foreach (var argument in namedType.TypeArguments)
				descriptor.TypeArguments.Add(argument.Name, CreateDescriptor(argument));
		}

		private void ResolveMetaData(ManifestTypeDescriptor descriptor, ITypeSymbol symbol)
		{
			foreach (var i in symbol.AllInterfaces)
			{
				if (string.Compare(i.ToDisplayString(), typeof(IEnumerable).FullName, false) == 0)
					descriptor.IsArray = true;
				else if (string.Compare(i.ToDisplayString(), typeof(IDictionary).FullName, false) == 0)
					descriptor.IsDictionary = true;

				if (descriptor.IsArray && descriptor.IsDictionary)
					break;
			}
		}

		private static bool ResolvePrimitive(ITypeSymbol symbol)
		{
			if (symbol.SpecialType == SpecialType.System_Boolean
				|| symbol.SpecialType == SpecialType.System_Byte
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
					Model = null;
					ClassDeclaration = null;
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
