using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using TomPIT.Reflection;

namespace TomPIT.Design.CodeAnalysis
{
	public static class ConstructorExtensions
	{
		public static List<ConstructorInfo> GetAllConstructors(this Type type)
		{
			var members = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Where(f => f.MemberType == MemberTypes.Constructor);
			var result = new List<ConstructorInfo>();

			foreach (var member in members)
			{
				if (member is ConstructorInfo constructor)
					result.Add(constructor);
			}

			return result;
		}

		public static ConstructorInfo GetConstructorInfo(this IMethodSymbol ms, SemanticModel model)
		{
			if (ms is null)
				return null;

			if (ms.IsExtensionMethod)
				ms = ms.GetConstructedReducedFrom();

			if (ms is null)
				return null;

			if (Type.GetType(ms.DeclaringTypeName()) is not Type type)
				return null;

			var methodName = ms.Name;
			var methodArgumentTypeNames = new List<string>();

			foreach (var i in ms.Parameters)
			{
				if (i.Type.DeclaringTypeName() is string declaringType)
					methodArgumentTypeNames.Add(declaringType);
			}

			var argumentTypes = methodArgumentTypeNames.Select(typeName => Type.GetType(typeName));
			var constructors = type.GetAllConstructors();

			foreach (var ctor in constructors)
			{
				if (ctor.GetParameters().Length != ms.Parameters.Length)
					continue;

				bool match = true;
				var parameters = ctor.GetParameters();

				for (var i = 0; i < methodArgumentTypeNames.Count; i++)
				{
					var at = Reflection.TypeExtensions.GetType(methodArgumentTypeNames[i]);

					if (at == null)
						continue;

					var pt = parameters[i].ParameterType;

					if (pt.IsGenericMethodParameter)
						continue;

					if (pt.IsInterface)
					{
						if (at != pt && !at.ImplementsInterface(pt))
						{
							match = false;
							break;
						}
					}
					else if (at != pt && !at.IsSubclassOf(pt))
					{
						match = false;
						break;
					}
				}

				if (match)
					return ctor;
			}

			return null;
		}
	}
}
