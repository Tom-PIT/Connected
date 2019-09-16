using System;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Messaging;
using TomPIT.Connectivity;
using TomPIT.Reflection;

namespace TomPIT.Compilation
{
	public static class CompilerExtensions
	{
		public static string ScriptName(this IText sourceCode, ITenant tenant)
		{
			if (sourceCode is IServerEvent)
			{
				var parent = sourceCode.Parent;
				var props = parent.GetType().GetProperties();

				foreach (var i in props)
				{
					var value = i.GetValue(parent);

					if (value == sourceCode)
						return string.Format("{0}.{1}.csx", parent.ToString(), i.Name);
				}
			}

			var att = sourceCode.GetType().FindAttribute<SyntaxAttribute>();

			if (att == null)
				return $"{sourceCode.Configuration().ComponentName()}.csx";

			var fileName = sourceCode.ToString();

			if (sourceCode is IConfiguration)
				fileName = $"{sourceCode.Configuration().ComponentName()}";

			if (string.Compare(att.Syntax, SyntaxAttribute.Razor, true) == 0)
				return $"{fileName}.cshtml";
			else
				return $"{fileName}.csx";
		}

		public static Guid ScriptId(this IText sourceCode)
		{
			return sourceCode.Id;
		}
	}
}
