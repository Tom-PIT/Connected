﻿using System.Collections.Generic;
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

		public IScriptManifestType Parse(ITypeSymbolDescriptor descriptor)
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

			var refLocation = new ScriptManifestSymbolLocation
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

				var source = new ScriptManifestSymbolReference
				{
					Address = Compiler.Manifest.GetId(script.Configuration().MicroService(), script.Configuration().Component, script.Id),
					Identifier = symbol.ToDisplayString(),
					Type = ResolveReferenceType(declared.Kind)
				};

				source.Location.StartCharacter = location.StartLinePosition.Character;
				source.Location.StartLine = location.StartLinePosition.Line;
				source.Location.EndCharacter = location.EndLinePosition.Character;
				source.Location.EndLine = location.EndLinePosition.Line;

				if(!Compiler.Manifest.SymbolReferences.TryGetValue(source, out HashSet<IScriptManifestSymbolLocation> items))
				{
					items = new HashSet<IScriptManifestSymbolLocation>(new ScriptManifestLocationComparer());

					Compiler.Manifest.SymbolReferences.Add(source, items);
				}

				items.Add(refLocation);
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

		public static bool IsPlatformClass(string name)
		{
			return string.Compare(name, "__ScriptInfo", false) == 0;
		}

		private static void SetLocation(IScriptManifestMember member, SyntaxNode node)
		{
			var span = node.GetLocation().GetLineSpan();

			member.Location.EndCharacter = span.EndLinePosition.Character;
			member.Location.EndLine = span.EndLinePosition.Line;
			member.Location.StartCharacter = span.StartLinePosition.Character;
			member.Location.StartLine = span.StartLinePosition.Line;
		}
	}
}