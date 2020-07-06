using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.Reflection;

namespace TomPIT.Design
{
	public static class DesignExtensions
	{
		public static string FileName(this IText text)
		{
			var value = text.ToString();

			var extension = text.GetType().FindAttribute<FileNameExtensionAttribute>();

			if (extension != null)
				return $"{value}.{extension.Extension}";

			var syntax = text.GetType().FindAttribute<SyntaxAttribute>();

			if (syntax == null)
				return value;

			if (string.Compare(syntax.Syntax, SyntaxAttribute.CSharp, true) == 0)
				return $"{value}.csx";
			else if (string.Compare(syntax.Syntax, SyntaxAttribute.Css, true) == 0)
				return $"{value}.css";
			else if (string.Compare(syntax.Syntax, SyntaxAttribute.Javascript, true) == 0)
				return $"{value}.jsm";
			else if (string.Compare(syntax.Syntax, SyntaxAttribute.Json, true) == 0)
				return $"{value}.json";
			else if (string.Compare(syntax.Syntax, SyntaxAttribute.Less, true) == 0)
				return $"{value}.less";
			else if (string.Compare(syntax.Syntax, SyntaxAttribute.Razor, true) == 0)
				return $"{value}.cshtml";
			else if (string.Compare(syntax.Syntax, SyntaxAttribute.Sql, true) == 0)
				return $"{value}.sql";
			else
				return value;
		}
	}
}
