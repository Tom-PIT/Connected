using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;

namespace TomPIT.Ide.TextServices.CSharp
{
	internal static class CSharpReflection
	{
		public static List<Type> ResolveTypes(List<Assembly> assemblies, string type)
		{
			var result = new List<Type>();

			foreach (var assembly in assemblies)
			{
				var types = assembly.GetTypes().Where(f => string.Compare(f.Name, type, true) == 0 && f.IsPublic);

				if (types != null && types.Count() > 0)
					result.AddRange(types);
			}

			return result;
		}

		public static List<MethodInfo> GetExtensionMethods(List<Assembly> assemblies, string name)
		{
			var result = new List<MethodInfo>();

			foreach (var assembly in assemblies)
			{
				var extensions = GetExtensionMethods(assembly, name);

				if (extensions != null && extensions.Count() > 0)
					result.AddRange(extensions);
			}

			return result;
		}

		private static IEnumerable<MethodInfo> GetExtensionMethods(Assembly assembly, string name)
		{
			var query = from type in assembly.GetTypes()
							where type.IsSealed && !type.IsGenericType && !type.IsNested
							from method in type.GetMethods(BindingFlags.Static
								 | BindingFlags.Public | BindingFlags.NonPublic)
							where method.IsDefined(typeof(ExtensionAttribute), false)
							&& string.Compare(method.Name, name, false) == 0
							select method;

			return query;
		}
	}
}
