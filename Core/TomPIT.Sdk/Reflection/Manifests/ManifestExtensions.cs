﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Design.CodeAnalysis;
using TomPIT.Development;
using TomPIT.Reflection.Manifests.Entities;
using TomPIT.Reflection.Manifests.Providers;

namespace TomPIT.Reflection.Manifests
{
	internal static class ManifestExtensions
	{
		public static IScriptDescriptor GetScript(ITenant tenant, IText text, Type hostType)
		{
			var service = tenant.GetService<ICompilerService>();

			if (hostType == null)
				return service.GetScript(text.Configuration().MicroService(), text);
			else
			{
				var methods = service.GetType().GetMethods().Where(f => string.Compare(f.Name, nameof(ICompilerService.GetScript), false) == 0);

				foreach (var method in methods)
				{
					if (method.IsGenericMethod)
					{
						var target = method.MakeGenericMethod(new Type[] { hostType });

						return target.Invoke(service, new object[] { text.Configuration().MicroService(), text }) as IScriptDescriptor;
					}
				}
			}

			return null;
		}

		public static bool HasErrors(this List<IDevelopmentError> diagnostics)
		{
			if (diagnostics.Count == 0)
				return false;

			return diagnostics.Count(f => f.Severity == DevelopmentSeverity.Error) > 0;
		}

		public static ManifestMember BindType(SemanticModel model, ISymbol symbol, List<ManifestMember> types)
		{
			if (symbol is INamedTypeSymbol namedType)
				return BindType(model, namedType, types);
			else if (symbol is TypeDeclarationSyntax typeDeclaration)
				return BindType(model, typeDeclaration, types);

			return null;
		}

		private static ManifestMember BindType(SemanticModel model, TypeDeclarationSyntax symbol, List<ManifestMember> types)
		{
			var name = symbol.Identifier.ValueText;

			var existing = types.FirstOrDefault(f => string.Compare(f.Type, name, true) == 0);

			if (existing != null)
				return existing;

			var manifestMember = new ManifestMember
			{
				Type = name
			};

			types.Add(manifestMember);

			foreach (var member in symbol.Members)
			{
				if (!(member is PropertyDeclarationSyntax pdx))
					continue;

				if (pdx.Modifiers.FirstOrDefault(f => string.Compare(f.ValueText, "public", false) == 0) == null)
					continue;

				var p = new ManifestProperty
				{
					Name = pdx.Identifier.ValueText,
					Type = CodeAnalysisExtentions.ToManifestTypeName(pdx.Type)
				};

				foreach (var accessor in pdx.AccessorList.Accessors)
				{
					if (string.Compare(accessor.Keyword.ValueText, "get", false) == 0)
						p.CanRead = true;
					else if (string.Compare(accessor.Keyword.ValueText, "set", false) == 0)
						p.CanWrite = true;
				}

				foreach (var attributeList in pdx.AttributeLists)
				{
					foreach (var attribute in attributeList.Attributes)
					{
						var att = ManifestAttributeResolver.Resolve(model, attribute);

						if (att != null)
							p.Attributes.Add(att);
					}
				}

				p.Documentation = ExtractDocumentation(pdx);

				manifestMember.Properties.Add(p);

				//BindType(model, model.GetTypeInfo(pdx.ex), types);
			}

			return manifestMember;
		}
		private static ManifestMember BindType(SemanticModel model, INamedTypeSymbol symbol, List<ManifestMember> types)
		{
			var current = symbol;

			if (symbol.TypeKind != TypeKind.Class)
				return null;

			var name = symbol.Name;

			var existing = types.FirstOrDefault(f => string.Compare(f.Type, name, true) == 0);

			if (existing != null)
				return existing;

			var manifestMember = new ManifestMember
			{
				Type = name
			};

			types.Add(manifestMember);

			while (current != null)
			{
				var members = current.GetMembers();

				foreach (var member in members)
				{
					var property = CreateProperty(model, member, types);

					if (property == null)
						continue;

					manifestMember.Properties.Add(property);
				}

				current = current.BaseType;
			}

			return manifestMember;
		}

		public static ManifestProperty CreateProperty(SemanticModel model, ISymbol symbol, List<ManifestMember> types)
		{
			if (!(symbol is IPropertySymbol property))
				return null;

			if (property.DeclaredAccessibility != Accessibility.Public)
				return null;

			var p = new ManifestProperty
			{
				Name = property.Name,
				Type = CodeAnalysisExtentions.ToManifestTypeName(property.Type)
			};

			if (property.GetMethod != null && property.GetMethod.DeclaredAccessibility == Accessibility.Public)
				p.CanRead = true;

			if (property.SetMethod != null && property.SetMethod.DeclaredAccessibility == Accessibility.Public)
				p.CanWrite = true;

			foreach (var attribute in property.GetAttributes())
			{
				var att = ManifestAttributeResolver.Resolve(model, attribute);

				if (att != null)
					p.Attributes.Add(att);
			}

			//p.Documentation = ExtractDocumentation(property);

			BindType(model, property.Type, types);

			return p;
		}

		public static string ExtractDocumentation(CSharpSyntaxNode node)
		{
			var trivias = node.GetLeadingTrivia();

			if (trivias == null || trivias.Count == 0)
				return null;

			var enumerator = trivias.GetEnumerator();

			while (enumerator.MoveNext())
			{
				var trivia = enumerator.Current;

				if (trivia.Kind().Equals(SyntaxKind.SingleLineDocumentationCommentTrivia))
				{
					var xml = trivia.GetStructure();

					if (xml == null)
						continue;

					var fullString = xml.ToFullString();
					var sb = new StringBuilder();
					string currentLine = null;

					using (var r = new StringReader(fullString))
					{
						while ((currentLine = r.ReadLine()) != null)
						{
							currentLine = currentLine.Trim();

							if (currentLine.StartsWith("///"))
								currentLine = currentLine.Substring(3).Trim();

							sb.AppendLine(currentLine);
						}
					}

					return sb.ToString();
				}
			}

			return null;
		}
	}
}