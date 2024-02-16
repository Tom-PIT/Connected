using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TomPIT.ComponentModel;

namespace TomPIT.Compilation
{
	public static class CompilerExtensions
	{
		public static string ResolvePath(this IText sourceCode)
		{
			var name = sourceCode.FileName;
			var ms = sourceCode.Configuration().MicroService();
			var microService = Tenant.GetService<IMicroServiceService>().Select(ms);

			if (sourceCode is IConfiguration)
				return $"{microService.Name}/{name}";
			else
			{
				var componentName = Tenant.GetService<IComponentService>().SelectComponent(sourceCode.Configuration().Component);

				return $"{microService.Name}/{componentName}/{name}";
			}
		}

		public static Guid ScriptId(this IText sourceCode)
		{
			return sourceCode.TextBlob;
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
