using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Scripting;
using TomPIT.Exceptions;
using TomPIT.Reflection;

namespace TomPIT.Compilation;
internal static class ScriptTypeResolver
{
	public static Type ResolveType(CompilerService service, Guid microService, IText sourceCode, string typeName, bool throwException)
	{
		var script = service.GetScript(new CompilerScriptArgs(microService, sourceCode));

		if (script is null)
		{
			if (throwException)
				throw new RuntimeException($"{SR.ErrTypeNotFound} ({typeName})");
			else
				return null;
		}

		if (script is not null && script.Assembly is null && script.Errors is not null && script.Errors.Any(f => f.Severity == DiagnosticSeverity.Error))
		{
			if (throwException)
				throw new CompilerException(script, sourceCode);
			else
				return null;
		}

		var result = ResolveTypeName(script.Assembly, sourceCode, typeName, throwException);

		if (result is null)
		{
			if (throwException)
				throw new RuntimeException($"{SR.ErrTypeNotFound} ({typeName})");
			else
				return null;
		}

		return result;
	}

	public static Type ResolveTypeName(string assembly, IText sourceCode, string typeName, bool throwException)
	{
		var ns = ResolveNamespace(sourceCode);
		var asm = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(f => string.Compare(f.ShortName(), assembly, true) == 0);

		if (asm is null)
			return null;

		if (ns is not null)
			typeName = $"{ns.Namespace}.{typeName}";

		if (!typeName.Contains("."))
			return asm.GetTypes().FirstOrDefault(f => string.Equals(f.Name, typeName, StringComparison.OrdinalIgnoreCase));

		var tokens = typeName.Split('.');
		var fullTypeName = new StringBuilder();

		fullTypeName.Append("Submission#0");

		foreach (var token in tokens)
			fullTypeName.Append($"+{token}");

		var results = asm.GetTypes().Where(f => string.Equals(f.FullName, fullTypeName.ToString(), StringComparison.OrdinalIgnoreCase));

		if (results.Count() > 1)
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(sourceCode.Configuration().MicroService());

			if (throwException)
				throw new RuntimeException($"{SR.ErrTypeMultipleMatch} ({ms.Name}/{sourceCode.Configuration().ComponentName()}/{typeName})");

			return null;
		}
		else if (!results.Any())
		{
			if (throwException)
				throw new RuntimeException($"{SR.ErrTypeNotFound} ({typeName})");

			return null;
		}

		return results.First();
	}

	private static INamespaceElement ResolveNamespace(IText sourceCode)
	{
		if (sourceCode is INamespaceElement nse && !string.IsNullOrWhiteSpace(nse.Namespace))
			return nse;

		IElement current = sourceCode;

		while (current is not null)
		{
			current = current.Parent?.Closest<INamespaceElement>();

			if (current is null)
				break;

			if (!string.IsNullOrWhiteSpace(((INamespaceElement)current).Namespace))
				return current as INamespaceElement;
		}

		return null;
	}

	public static List<string> QuerySubClasses(CompilerService service, IScriptConfiguration script)
	{
		var result = new List<string>();

		var scriptType = ResolveType(service, script.MicroService(), script, script.ComponentName(), true);

		if (scriptType == null)
			return null;

		foreach (var item in service.Scripts.Items)
		{
			var ms = item.MicroService();
			var name = item.ComponentName();

			var type = ResolveType(service, ms, item, name, false);

			if (type == null)
				continue;

			type = type.BaseType;

			while (type != null)
			{
				if (ScriptTypeComparer.Compare(scriptType, type))
				{
					var microService = Tenant.GetService<IMicroServiceService>().Select(ms);

					if (microService == null)
						continue;

					result.Add($"{microService.Name}/{name}");
					break;
				}
			}
		}

		return result;
	}

	public static IComponent ResolveComponent(Type type)
	{
		if (type is null)
			return null;

		var typeInfo = CompilerExtensions.ResolveScriptInfoType(type.Assembly);

		if (typeInfo is null)
			return null;

		var sourceTypes = (List<SourceTypeDescriptor>)typeInfo.GetProperty("SourceTypes").GetValue(null);

		var componentMatch = sourceTypes.FirstOrDefault(typeDescriptor => TypeMatches(typeDescriptor, type));

		if (componentMatch is not null)
		{
			var cmp = Tenant.GetService<IComponentService>().SelectComponent(componentMatch.Component);

			if (cmp is not null)
				return cmp;
		}

		var component = (Guid)typeInfo.GetProperty("Component").GetValue(null);

		return Tenant.GetService<IComponentService>().SelectComponent(component);
	}

	private static bool TypeMatches(SourceTypeDescriptor typeDescriptor, Type type)
	{
		return NameMatches(typeDescriptor.TypeName, type.Name) && NamespaceMatches(type.DeclaringType.FullName, typeDescriptor.ContainingType);
	}

	private static bool NameMatches(string name1, string name2)
	{
		return string.Compare(name1, name2) == 0;
	}

	private static bool NamespaceMatches(string namespace1, string namespace2)
	{
		var elements1 = namespace1.Split('.', '+').SkipWhile(e => e.StartsWith("Submission#0") || string.IsNullOrEmpty(e));
		var elements2 = namespace2.Split('.', '+').SkipWhile(e => e.StartsWith("Submission#0") || string.IsNullOrEmpty(e));

		return elements1.SequenceEqual(elements2);
	}

	public static IMicroService ResolveMicroService(Type type)
	{
		if (type is null)
			return null;

		var typeInfo = type.Assembly.DefinedTypes.FirstOrDefault(f => string.Compare(f.Name, CompilerService.ScriptInfoClassName, false) == 0);

		if (typeInfo is null)
			return null;

		var ms = (Guid)typeInfo.GetProperty("MicroService").GetValue(null);

		return Tenant.GetService<IMicroServiceService>().Select(ms);
	}
}
