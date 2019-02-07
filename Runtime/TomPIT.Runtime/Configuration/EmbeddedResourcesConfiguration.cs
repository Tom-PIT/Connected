using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Reflection;
using TomPIT.Services;

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
			{
				var t = Types.GetType(i);

				if (t == null)
					continue;

				assemblies.Add(t.Assembly);
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
	}
}