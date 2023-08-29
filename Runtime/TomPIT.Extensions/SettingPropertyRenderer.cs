using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using TomPIT.Compilation;
using TomPIT.Middleware;

namespace TomPIT
{
	internal class SettingPropertyRenderer : PropertyRenderer
	{
		public SettingPropertyRenderer(IMiddlewareContext context, string setting, string propertyName) : base(context, propertyName)
		{
			Properties = SyntaxBrowser.QueryProperties(setting);
		}

		protected override ImmutableArray<PropertyDeclarationSyntax> Properties { get; }
		private string Setting { get; }
	}
}
