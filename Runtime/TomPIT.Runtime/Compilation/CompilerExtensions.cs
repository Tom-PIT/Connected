using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Resources;

namespace TomPIT.Compilation
{
	public static class CompilerExtensions
	{
		public const char PathSeparatorChar = '/';

		public static string ResolvePath(this IText sourceCode)
		{
			var name = sourceCode.FileName;
			var ms = sourceCode.Configuration().MicroService();
			var microService = Tenant.GetService<IMicroServiceService>().Select(ms);

			if (sourceCode is IConfiguration)
			{
				return $"{microService.Name}/{name}";
			}
			else if (sourceCode.Configuration() is ITextConfiguration textConfiguration)
			{
				return ResolveFileName(textConfiguration);
			}
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

		private static string? ResolveFileName(this ITextConfiguration configuration)
		{
			var component = Tenant.GetService<IComponentService>().SelectComponent(configuration.Component);

			if (component is null)
				return null;

			var ms = Tenant.GetService<IMicroServiceService>().Select(component.MicroService);

			if (ms is null)
				return null;

			var folderPath = FolderPath(component);
			var result = new List<string>();

			result.Add(ms.Name);

			if (folderPath is not null)
				result.Add(folderPath);

			result.Add(configuration.FileName);

			return string.Join(PathSeparatorChar, result);
		}

		private static string? FolderPath(this IComponent component)
		{
			if (component.Folder == Guid.Empty)
				return null;

			var result = new List<string>();
			var folder = Tenant.GetService<IComponentService>().SelectFolder(component.Folder);

			result.Add(folder.Name);

			while (folder.Parent != Guid.Empty)
			{
				folder = Tenant.GetService<IComponentService>().SelectFolder(folder.Parent);

				result.Insert(0, folder.Name);
			}

			return string.Join(PathSeparatorChar, result);
		}
	}
}
