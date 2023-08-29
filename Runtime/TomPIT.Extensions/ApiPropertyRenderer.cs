using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using TomPIT.Compilation;
using TomPIT.Middleware;

namespace TomPIT
{
	internal class ApiPropertyRenderer : PropertyRenderer
	{
		public ApiPropertyRenderer(IMiddlewareContext context, string api, string propertyName) : base(context, propertyName)
		{
			Properties = SyntaxBrowser.QueryProperties(api);
		}

		protected override ImmutableArray<PropertyDeclarationSyntax> Properties { get; }
	}
}
