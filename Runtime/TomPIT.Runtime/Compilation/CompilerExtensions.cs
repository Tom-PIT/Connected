using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TomPIT.Annotations.Design;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Messaging;
using TomPIT.Connectivity;
using TomPIT.Reflection;

namespace TomPIT.Compilation
{
	public static class CompilerExtensions
	{
		public static string ResolvePath(this IText sourceCode, ITenant tenant)
		{
			var name = sourceCode.FileName;
			var ms = sourceCode.Configuration().MicroService();
			var microService = tenant.GetService<IMicroServiceService>().Select(ms);

			if (sourceCode is IConfiguration)
				return $"{microService.Name}/{name}";
			else
			{
				var componentName = tenant.GetService<IComponentService>().SelectComponent(sourceCode.Configuration().Component);

				return $"{microService.Name}/{componentName}/{name}";
			}
		}

		[Obsolete("Please use IText.FileName instead.")]
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

		public static Type ResolveScriptInfoType(Assembly assembly)
		{
			return assembly.DefinedTypes.FirstOrDefault(f => string.Equals(f.Name, CompilerService.ScriptInfoClassName, StringComparison.Ordinal));
		}

		public static bool HasScriptReference(Assembly assembly, Guid script)
		{
			var type = ResolveScriptInfoType(assembly);

			if (type is null)
				return false;

			var property = type.GetProperty("SourceTypes", BindingFlags.Static | BindingFlags.Public);

			if (property.GetValue(null) is not List<SourceTypeDescriptor> items || !items.Any())
				return false;

			return items.FirstOrDefault(f => f.Script == script) is not null;
		}
	}
}
