using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using TomPIT.Design;

namespace TomPIT.Configuration
{
	internal static class EmbeddedResourcesConfiguration
	{
		public static void Configure(IWebHostEnvironment environment, StaticFileOptions options)
		{
			var assemblies = new List<Assembly>();

			foreach (var i in Tenant.GetService<IDesignService>().QueryDesigners())
				RegisterAssembly(assemblies, i);

			foreach (var i in Instance.Plugins)
			{
				foreach (var j in i.GetEmbeddedResources())
					RegisterAssemblyFromName(assemblies, j);
			}

			options = options ?? throw new ArgumentNullException(nameof(options));

			options.ContentTypeProvider = options.ContentTypeProvider ?? new FileExtensionContentTypeProvider();

			if (options.FileProvider == null && environment.WebRootFileProvider == null)
				throw new InvalidOperationException("Missing FileProvider.");

			options.FileProvider = options.FileProvider ?? environment.WebRootFileProvider;

			var basePath = "wwwroot";

			var fileProviders = new List<IFileProvider>
			{
				options.FileProvider
			};

			foreach (var i in assemblies)
			{
				try
				{
					fileProviders.Add(new ManifestEmbeddedFileProvider(i, basePath));
				}
				catch { }
			}

			options.FileProvider = new CompositeFileProvider(fileProviders.ToArray());
		}

		private static void RegisterAssemblyFromName(List<Assembly> assemblies, string j)
		{
			var path = Shell.ResolveAssemblyPath(j);

			if (path == null)
				return;

			var asmName = AssemblyName.GetAssemblyName(path);
			var asm = AssemblyLoadContext.Default.LoadFromAssemblyName(asmName);

			if (asm != null)
				assemblies.Add(asm);
		}

		private static void RegisterAssembly(List<Assembly> assemblies, string type)
		{
			var t = Reflection.TypeExtensions.GetType(type);

			if (t == null)
				return;

			assemblies.Add(t.Assembly);
		}
	}
}