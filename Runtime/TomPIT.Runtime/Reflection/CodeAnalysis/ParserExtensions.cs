using System.Collections.Immutable;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using TomPIT.Design.CodeAnalysis;

namespace TomPIT.Reflection.CodeAnalysis
{
	internal static class ParserExtensions
	{
		public static string ParseDocumentation(this CSharpSyntaxNode node)
		{
			if (node == null)
				return null;

			var trivias = node.GetLeadingTrivia();

			if (trivias.Count == 0)
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
					string currentLine;

					using var r = new StringReader(fullString);

					while ((currentLine = r.ReadLine()) != null)
					{
						currentLine = currentLine.Trim();

						if (currentLine.StartsWith("///"))
							currentLine = currentLine.Substring(3).Trim();

						sb.AppendLine(currentLine);
					}

					return sb.ToString();
				}
			}

			return null;
		}

		public static bool IsBrowsable(this ImmutableArray<AttributeData> attributes)
		{
			foreach (var attribute in attributes)
			{
				if (attribute.AttributeClass == null || !attribute.AttributeClass.IsOfType(typeof(System.ComponentModel.BrowsableAttribute)))
					continue;

				if (attribute.ConstructorArguments.Length == 0)
					return true;

				return Types.Convert<bool>(attribute.ConstructorArguments[0].Value);
			}

			return true;
		}
	}
}
