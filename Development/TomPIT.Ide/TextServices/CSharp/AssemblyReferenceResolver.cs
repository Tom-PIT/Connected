using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;

namespace TomPIT.Ide.TextServices.CSharp
{
	internal static class AssemblyReferenceResolver
	{
		public static Assembly Resolve(MetadataReference reference)
		{
			var portable = reference as PortableExecutableReference;

			if (portable == null)
				return null;

			if (string.IsNullOrEmpty(portable.FilePath))
			{
				//var tokens = portable.Key.Split('/');
				//var ms = MicroService;
				//var name = reference.Key;

				//if (tokens.Length > 1)
				//{
				//	ms = Tenant.GetService<IMicroServiceService>().Select(tokens[0]).Token;
				//	name = tokens[1];
				//}

				//return AssemblyResolver.LoadDependency(Tenant, ms, name);
				throw new NotImplementedException();
			}
			else
			{
				var name = AssemblyLoadContext.GetAssemblyName(portable.FilePath);

				if (name == null)
					return null;

				return AssemblyLoadContext.Default.LoadFromAssemblyName(name);
			}
		}

		public static List<Assembly> ResolveReferences(Microsoft.CodeAnalysis.Compilation compilation)
		{
			var result = new List<Assembly>();

			foreach (var reference in compilation.References)
			{
				var assembly = Resolve(reference);

				if (assembly != null)
					result.Add(assembly);
			}

			return result;
		}
	}
}
