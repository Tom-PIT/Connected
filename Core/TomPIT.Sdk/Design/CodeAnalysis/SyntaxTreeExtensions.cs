using System.Linq;
using Microsoft.CodeAnalysis;

namespace TomPIT.Design.CodeAnalysis
{
	public static class SyntaxTreeExtensions
	{
		public static SyntaxNode ResolveNode(this ISymbol symbol)
		{
			if (!symbol.Locations.Any())
				return null;

			return symbol.ResolveNode(symbol.Locations[0].SourceTree, false);
		}
		public static SyntaxNode ResolveNode(this ISymbol symbol, SyntaxTree syntaxTree, bool sourceFileOnly)
		{
			if (!symbol.Locations.Any())
				return null;

			if (symbol.Locations.FirstOrDefault(f => f.SourceTree == syntaxTree) is Location local)
				return local.SourceTree?.GetRoot()?.FindNode(local.SourceSpan);

			if (sourceFileOnly)
				return null;

			foreach (var location in symbol.Locations)
			{
				if (!location.IsInSource)
					continue;

				return location.SourceTree?.GetRoot()?.FindNode(location.SourceSpan);
			}

			return null;
		}
	}
}
