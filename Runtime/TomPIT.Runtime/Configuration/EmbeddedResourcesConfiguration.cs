using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using TomPIT.Runtime.Configuration;

namespace TomPIT.Configuration
{
	internal class EmbeddedResourcesConfiguration : IPostConfigureOptions<StaticFileOptions>
	{
		public EmbeddedResourcesConfiguration(IHostingEnvironment environment)
		{
			Environment = environment;
		}

		public IHostingEnvironment Environment { get; }

		public void PostConfigure(string name, StaticFileOptions options)
		{
			var assemblies = new List<Assembly>();

			foreach (var i in Shell.GetConfiguration<IClientSys>().Designers)
				RegisterAssembly(assemblies, i);

			foreach (var i in Instance.Plugins)
			{
				foreach (var j in i.GetEmbeddedResources())
					RegisterAssemblyFromName(assemblies, j);
			}

			name = name ?? throw new ArgumentNullException(nameof(name));
			options = options ?? throw new ArgumentNullException(nameof(options));

			options.ContentTypeProvider = options.ContentTypeProvider ?? new FileExtensionContentTypeProvider();

			if (options.FileProvider == null && Environment.WebRootFileProvider == null)
				throw new InvalidOperationException("Missing FileProvider.");

			options.FileProvider = options.FileProvider ?? Environment.WebRootFileProvider;

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

		private void RegisterAssemblyFromName(List<Assembly> assemblies, string j)
		{
			var path = Shell.ResolveAssemblyPath(j);

			if (path == null)
				return;

			var asmName = AssemblyName.GetAssemblyName(path);
			var asm = AssemblyLoadContext.Default.LoadFromAssemblyName(asmName);

			if (asm != null)
				assemblies.Add(asm);
		}

		private void RegisterAssembly(List<Assembly> assemblies, string type)
		{
			var t = Reflection.TypeExtensions.GetType(type);

			if (t == null)
				return;

			assemblies.Add(t.Assembly);
		}
	}
}