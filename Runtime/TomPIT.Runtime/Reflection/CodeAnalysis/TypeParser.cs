using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using TomPIT.ComponentModel;
using TomPIT.Design.CodeAnalysis;

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

			if (ClassExtensions.IsPlatformClass(type.Name))
				return null;

			var result = new TypeSymbolDescriptor
			{
				Name = type.Name,
				Symbol = type,
				Node = node,
				Model = Compiler.Model,
				ContainingType = type.ContainingType?.ToDisplayString()
			};

			return result;
		}

		public IScriptManifestType Parse(ITypeSymbolDescriptor descriptor)
		{
			var result = CreateTypeInstance(descriptor);

			foreach (var member in descriptor.Symbol.GetMembers())
			{
				if (member is IPropertySymbol property)
					ParseSymbol(result.Item2, property);
				else if (member is IFieldSymbol field)
					ParseSymbol(result.Item2, field);
				else if (member is BaseTypeDeclarationSyntax)
					continue;
				else
					ParseNode(member);
			}

			if (result.Item1 != null)
				result.Item1.ProcessManifestType(descriptor, result.Item2);

			return result.Item2;
		}

		private (IScriptManifestProvider, IScriptManifestType) CreateTypeInstance(ITypeSymbolDescriptor descriptor)
		{
			IScriptManifestType type = null;
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
				type = new ScriptManifestType();

			type.Name = descriptor.Symbol.Name;
			type.Documentation = ((CSharpSyntaxNode)descriptor.Node).ParseDocumentation();
			type.ContainingType = descriptor.ContainingType;

			SetLocation(type, descriptor.Node);

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

			var model = Compiler.Compilation.GetSemanticModel(identifier.SyntaxTree);
			var symbol = model.GetSymbolInfo(identifier).Symbol;

			if (symbol == null || symbol.IsImplicitlyDeclared)
				return;

			if (symbol is ITypeSymbol || symbol is IPropertySymbol || symbol is IFieldSymbol || symbol is IMethodSymbol)
				ParseSourceReferences(symbol, identifier.Span);
		}

		private void ParseSymbol(IScriptManifestType type, IFieldSymbol symbol)
		{
			if (symbol.IsImplicitlyDeclared)
				return;

			ParseSourceReferences(symbol, symbol.Type);

			var node = ResolveNode(symbol) as CSharpSyntaxNode;

			var member = new ScriptManifestField
			{
				Name = symbol.Name,
				Type = symbol.Type.ToDisplayString(),
				IsConstant = symbol.IsConst,
				IsPublic = symbol.DeclaredAccessibility == Accessibility.Public,
				Documentation = node is not null ? node.ParseDocumentation() : null
			};

			SetLocation(member, node);
			ParseAttributes(symbol, member.Attributes);

			type.Members.Add(member);
		}
		private void ParseSymbol(IScriptManifestType type, IPropertySymbol symbol)
		{
			ParseSourceReferences(symbol, symbol.Type);

			if (symbol.IsIndexer)
				return;

			var node = ResolveNode(symbol) as CSharpSyntaxNode;

			var member = new ScriptManifestProperty
			{
				Name = symbol.Name,
				CanRead = symbol.GetMethod != null,
				CanWrite = symbol.SetMethod != null,
				Type = symbol.Type.ToDisplayString(),
				IsPublic = symbol.DeclaredAccessibility == Accessibility.Public,
				Documentation = node is not null ? node.ParseDocumentation() : null
			};

			SetLocation(member, node);
			ParseAttributes(symbol, member.Attributes);

			type.Members.Add(member);
		}

		private void ParseSourceReferences(ISymbol declared, ITypeSymbol symbol)
		{
			if (symbol is not INamedTypeSymbol namedType)
				return;

			if (!symbol.Locations.IsDefaultOrEmpty)
				ParseSourceReferences(symbol, symbol.Locations[0].SourceSpan);

			var arguments = namedType.TypeArguments;

			foreach (var argument in arguments)
				ParseSourceReferences(declared, argument);
		}

		private void ParseSourceReferences(ISymbol symbol, TextSpan span)
		{
			if (symbol.Locations.IsDefaultOrEmpty || symbol.DeclaringSyntaxReferences.IsDefaultOrEmpty)
				return;

			foreach (var reference in symbol.DeclaringSyntaxReferences)
			{
				var script = Compiler.ResolveScript(reference.SyntaxTree.FilePath);

				if (script == null || symbol.Locations.IsDefaultOrEmpty)
					continue;

				var source = new ScriptManifestSymbolReference
				{
					Address = Compiler.Manifest.GetId(script.Configuration().MicroService(), script.Configuration().Component, script.Id),
					Identifier = symbol.ToDisplayString(),
					Type = ResolveReferenceType(symbol.Kind)
				};

				if (!Compiler.Manifest.SymbolReferences.TryGetValue(source, out HashSet<IScriptManifestSymbolLocation> items))
				{
					items = new HashSet<IScriptManifestSymbolLocation>(new ScriptManifestLocationComparer());

					Compiler.Manifest.SymbolReferences.Add(source, items);
				}

				items.Add(new ScriptManifestSymbolLocation
				{
					Start =span.Start,
					End = span.End
				});
			}
		}

		private ScriptManifestSourceReferenceType ResolveReferenceType(SymbolKind kind)
		{
			switch (kind)
			{
				case SymbolKind.Event:
					return ScriptManifestSourceReferenceType.Event;
				case SymbolKind.Field:
					return ScriptManifestSourceReferenceType.Field;
				case SymbolKind.Local:
					return ScriptManifestSourceReferenceType.Local;
				case SymbolKind.Method:
					return ScriptManifestSourceReferenceType.Method;
				case SymbolKind.NamedType:
					return ScriptManifestSourceReferenceType.Type;
				case SymbolKind.Property:
					return ScriptManifestSourceReferenceType.Property;
				default:
					return ScriptManifestSourceReferenceType.Other;
			}
		}
		private void ParseAttributes(ISymbol symbol, List<IScriptManifestAttribute> items)
		{
			var attributes = symbol.GetAttributes();

			foreach (var attribute in attributes)
			{
				var att = AttributeParser.Parse(attribute);

				if (att != null)
					items.Add(att);
			}
		}

		private static void SetLocation(IScriptManifestMember member, SyntaxNode node)
		{
			member.Location.Start = node.Span.Start;
			member.Location.End = node.Span.End;
		}
	}
}
