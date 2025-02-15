﻿using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;
using TomPIT.Reflection;
using TomPIT.Storage;

namespace TomPIT.Compilation
{
	public class ScriptResolver : SourceReferenceResolver
	{
		public ScriptResolver(ITenant tenant, Guid microService)
		{
			Tenant = tenant;
			MicroService = microService;
		}

		private ITenant Tenant { get; }
		private Guid MicroService { get; }

		protected bool Equals(ScriptResolver other)
		{
			return Equals(Tenant, other.Tenant);
		}

		public override bool Equals(object obj)
		{
			if (obj is ScriptResolver resolver2)
				return Equals(this, resolver2);

			return false;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = 37;

				hashCode = (hashCode * 397) ^ (Tenant?.GetHashCode() ?? 0);

				return hashCode;
			}
		}
		public override string? NormalizePath(string path, string baseFilePath)
		{
			var sourceFiles = Shell.Configuration.GetRequiredSection("sourceFiles").GetValue<string>("folder");

			if (path.StartsWith(sourceFiles))
				return path;

			var text = Tenant.GetService<IDiscoveryService>().Configuration.Find(path);

			if (text is null)
				return path;

			return Path.Combine(sourceFiles, text.Configuration().MicroService().ToString(), $"{text.TextBlob}-{BlobTypes.SourceText}.txt");
		}

		public override Stream OpenRead(string resolvedPath)
		{
			var sourceCode = Tenant.GetService<IDiscoveryService>().Configuration.Find(resolvedPath);

			if (sourceCode is null)
				return new MemoryStream(Encoding.UTF8.GetBytes(string.Empty));

			var content = Tenant.GetService<IComponentService>().SelectText(sourceCode.Configuration().MicroService(), sourceCode);

			if (string.IsNullOrWhiteSpace(content))
				return new MemoryStream(Encoding.UTF8.GetBytes(string.Empty));

			return new MemoryStream(Encoding.UTF8.GetBytes(NamespaceRewriter.Rewrite(content)));
		}

		public override string ResolveReference(string path, string baseFilePath)
		{
			if (path.EndsWith(".csx"))
				return path;

			return $"{path}.csx";
		}

		private static string ValidatePath(IMicroService origin, IMicroService baseMicroService, string resolvedPath)
		{
			var target = baseMicroService ?? origin;

			var ms = resolvedPath.Split('/')[0];

			target.ValidateMicroServiceReference(ms);

			return resolvedPath;
		}

		
	}
}
