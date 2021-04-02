using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.ComponentModel;

namespace TomPIT.Reflection.CodeAnalysis
{
	internal class TypeParser : ParserBase
	{
		private AttributeParser _attributeParser;
		public TypeParser(IScriptManifestCompiler compiler) : base(compiler)
		{

		}

		private AttributeParser AttributeParser => _attributeParser ??= new AttributeParser(Compiler);

		public TypeSymbolDescriptor Parse(SyntaxNode node)
		{
			var symbol = Compiler.Model.GetDeclaredSymbol(node);

			if (symbol == null || symbol is not INamedTypeSymbol type)
				return null;

			if (IsPlatformClass(type.Name))
				return null;

			var result = new TypeSymbolDescriptor
			{
				Name = type.Name,
				Symbol = type,
				Node = node,
				Model = Compiler.Model
			};

			return result;
		}

		public IManifestType Parse(ITypeSymbolDescriptor descriptor)
		{
			var result = CreateTypeInstance(descriptor);

			foreach (var member in descriptor.Symbol.GetMembers())
			{
				if (member is IPropertySymbol property)
					ParseSymbol(result.Item2, property);
				else if (member is IFieldSymbol field)
					ParseSymbol(result.Item2, field);
				else
					ParseNode(member);
			}

			if (result.Item1 != null)
				result.Item1.ProcessManifestType(descriptor, result.Item2);

			return result.Item2;
		}

		private (IScriptManifestProvider, IManifestType) CreateTypeInstance(ITypeSymbolDescriptor descriptor)
		{
			IManifestType type = null;
			IScriptManifestProvider scriptProvider = null;

			foreach (var provider in Compiler.Providers)
			{
				type = provider.CreateTypeInstance(descriptor);

				if (type is not null)
				{
					scriptProvider = provider;
					break;
				}
			}

			if (type == null)
				type = new ManifestType();

			type.Name = descriptor.Symbol.Name;
			type.Documentation = ((CSharpSyntaxNode)descriptor.Node).ParseDocumentation();

			return (scriptProvider, type);
		}

		private SyntaxNode ResolveNode(ISymbol symbol)
		{
			if (!symbol.Locations.Any())
				return null;

			foreach (var location in symbol.Locations)
			{
				if (!location.IsInSource)
					continue;

				return location.SourceTree?.GetRoot()?.FindNode(location.SourceSpan);
			}

			return null;
		}
		private void ParseNode(ISymbol symbol)
		{
			ParseNode(ResolveNode(symbol));
		}

		private void ParseNode(SyntaxNode node)
		{
			if (node == null)
				return;

			if (node is not IdentifierNameSyntax identifier)
			{
				foreach (var child in node.ChildNodes())
					ParseNode(child);

				return;
			}

			var symbol = Compiler.Model.GetSymbolInfo(identifier).Symbol;

			if (symbol == null || symbol.IsImplicitlyDeclared)
				return;

			if (symbol is ITypeSymbol type)
				ParseSourceReferences(symbol, type);
			else if (symbol is IPropertySymbol property)
				ParseSourceReferences(symbol, property);
			else if (symbol is IMethodSymbol method)
				ParseSourceReferences(symbol, method);
		}

		private void ParseSymbol(IManifestType type, IFieldSymbol symbol)
		{
			if (symbol.IsImplicitlyDeclared)
				return;

			ParseSourceReferences(symbol, symbol.Type);

			var node = ResolveNode(symbol) as CSharpSyntaxNode;

			var member = new ManifestField
			{
				Name = symbol.Name,
				Type = symbol.Type.ToDisplayString(),
				IsConstant = symbol.IsConst,
				Documentation = node is not null ? node.ParseDocumentation() : null
			};
			
			ParseAttributes(symbol, member.Attributes);

			type.Members.Add(member);
		}
		private void ParseSymbol(IManifestType type, IPropertySymbol symbol)
		{
			ParseSourceReferences(symbol, symbol.Type);

			if (symbol.IsIndexer || symbol.DeclaredAccessibility != Accessibility.Public)
				return;

			var node = ResolveNode(symbol) as CSharpSyntaxNode;

			var member = new ManifestProperty
			{
				Name = symbol.Name,
				CanRead = symbol.GetMethod != null,
				CanWrite = symbol.SetMethod != null,
				Type = symbol.Type.ToDisplayString(),
				Documentation = node is not null ? node.ParseDocumentation() : null
			};

			ParseAttributes(symbol, member.Attributes);

			type.Members.Add(member);
		}

		private void ParseSourceReferences(ISymbol declared, IMethodSymbol symbol)
		{
			ParseSourceReferences(declared, symbol, symbol.DeclaringSyntaxReferences);
		}

		private void ParseSourceReferences(ISymbol declared,  IPropertySymbol symbol)
		{
			ParseSourceReferences(declared, symbol, symbol.DeclaringSyntaxReferences);
		}
		private void ParseSourceReferences(ISymbol declared, ITypeSymbol symbol)
		{
			if (symbol is not INamedTypeSymbol namedType)
				return;

			ParseSourceReferences(declared, symbol, symbol.DeclaringSyntaxReferences);

			var arguments = namedType.TypeArguments;

			foreach (var argument in arguments)
				ParseSourceReferences(declared, argument);
		}

		private void ParseSourceReferences(ISymbol declared, ISymbol symbol, ImmutableArray<SyntaxReference> references)
		{
			if (declared.Locations.IsDefaultOrEmpty || references.IsDefaultOrEmpty)
				return;

			var mappedLocation = declared.Locations[0].GetMappedLineSpan();

			var refLocation = new ManifestSymbolLocation
			{
				StartCharacter = mappedLocation.StartLinePosition.Character,
				StartLine = mappedLocation.StartLinePosition.Line,
				EndCharacter = mappedLocation.EndLinePosition.Character,
				EndLine = mappedLocation.EndLinePosition.Line
			};

			foreach (var reference in references)
			{
				var script = Compiler.ResolveScript(reference.SyntaxTree.FilePath);

				if (script == null || symbol.Locations.IsDefaultOrEmpty)
					continue;

				var location = symbol.Locations[0].GetMappedLineSpan();

				var source = new ManifestSymbolReference
				{
					Address = Compiler.Manifest.GetId(script.Configuration().MicroService(), script.Configuration().Component, script.Id),
					Identifier = symbol.ToDisplayString(),
					Type = ResolveReferenceType(declared.Kind)
				};

				source.Location.StartCharacter = location.StartLinePosition.Character;
				source.Location.StartLine = location.StartLinePosition.Line;
				source.Location.EndCharacter = location.EndLinePosition.Character;
				source.Location.EndLine = location.EndLinePosition.Line;

				if(!Compiler.Manifest.SymbolReferences.TryGetValue(source, out HashSet<IManifestSymbolLocation> items))
				{
					items = new HashSet<IManifestSymbolLocation>(new ManifestLocationComparer());

					Compiler.Manifest.SymbolReferences.Add(source, items);
				}

				items.Add(refLocation);
			}
		}

		private ManifestSourceReferenceType ResolveReferenceType(SymbolKind kind)
		{
			switch (kind)
			{
				case SymbolKind.Event:
					return ManifestSourceReferenceType.Event;
				case SymbolKind.Field:
					return ManifestSourceReferenceType.Field;
				case SymbolKind.Local:
					return ManifestSourceReferenceType.Local;
				case SymbolKind.Method:
					return ManifestSourceReferenceType.Method;
				case SymbolKind.NamedType:
					return ManifestSourceReferenceType.Type;
				case SymbolKind.Property:
					return ManifestSourceReferenceType.Property;
				default:
					return ManifestSourceReferenceType.Other;
			}
		}
		private void ParseAttributes(ISymbol symbol, List<IManifestAttribute> items)
		{
			var attributes = symbol.GetAttributes();

			foreach (var attribute in attributes)
			{
				var att = AttributeParser.Parse(attribute);

				if (att != null)
					items.Add(att);
			}
		}

		public static bool IsPlatformClass(string name)
		{
			return string.Compare(name, "__ScriptInfo", false) == 0;
		}

		public static ITypeSymbol LookupBaseType(INamedTypeSymbol type, SemanticModel model, string baseTypeName)
		{
			if (type == null)
				return null;

			var displayName = type.ToDisplayName();

			if (string.Compare(displayName, baseTypeName, false) == 0)
				return type;

			foreach (var itf in type.AllInterfaces)
			{
				displayName = itf.ToDisplayName();

				if (string.Compare(displayName, baseTypeName, false) == 0)
					return itf;
			}

			if (type.BaseType == null)
				return null;

			return LookupBaseType(type.BaseType, model, baseTypeName);
		}
	}
}
