using System;
using System.Collections.Generic;
using System.Linq;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Development;

namespace TomPIT.Reflection.Manifests
{
	internal static class ManifestExtensions
	{
		public static IScriptDescriptor GetScript(ITenant tenant, IText text, Type hostType)
		{
			var service = tenant.GetService<ICompilerService>();

			if (hostType == null)
				return service.GetScript(text.Configuration().MicroService(), text);
			else
			{
				var methods = service.GetType().GetMethods().Where(f => string.Compare(f.Name, nameof(ICompilerService.GetScript), false) == 0);

				foreach (var method in methods)
				{
					if (method.IsGenericMethod)
					{
						var target = method.MakeGenericMethod(new Type[] { hostType });

						return target.Invoke(service, new object[] { text.Configuration().MicroService(), text }) as IScriptDescriptor;
					}
				}
			}

			return null;
		}

		public static bool HasErrors(this List<IDevelopmentError> diagnostics)
		{
			if (diagnostics.Count == 0)
				return false;

			return diagnostics.Count(f => f.Severity == DevelopmentSeverity.Error) > 0;
		}
	}
}
