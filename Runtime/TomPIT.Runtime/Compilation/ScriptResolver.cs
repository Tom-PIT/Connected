﻿using Microsoft.CodeAnalysis;
using System;
using System.IO;
using System.Linq;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.IoC;
using TomPIT.Connectivity;
using TomPIT.Reflection;

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
		public override string NormalizePath(string path, string baseFilePath)
		{
			if (path.Contains(":"))
				return path.Split(':')[1];

			return path;
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
			/*
			* possible references:
			* --------------------
			* - Microservice/Script (2)
			* - Microservice/IoCContainer/Operation (3)
			* - Microservice/Api/Operation (3)
			* obsolete:
			* - Script (1)
			* - Api/Operation (2)
			* - IoCContainer/Operation (2)
			*/

			var resolvedPath = path;

			var hasBaseFile = !string.IsNullOrWhiteSpace(baseFilePath);

			var ms = Tenant.GetService<IMicroServiceService>().Select(MicroService);

			var tokens = resolvedPath.Split(new char[] { '/' }, 3);

			var fileName = tokens[^1];

			if (!Path.HasExtension(fileName))
				tokens[^1] = $"{fileName}.csx";

			IMicroService baseMs = null;

			if (hasBaseFile)
			{
				var root = baseFilePath.Split(':');
				var baseMsToken = root.Length == 1
					 ? root[0].Split('/')[0]
					 : root[1].Split('/')[0];

				if (baseMsToken.EndsWith(".csx"))
					baseMs = ms;
				else
					baseMs = Tenant.GetService<IMicroServiceService>().Select(baseMsToken) ?? ms;
			}

			switch (tokens.Length)
			{
				case 3: //Fully qualified as ms/component/entry
					{
						resolvedPath = $"{tokens[0]}/{tokens[1]}/{tokens[2]}";
						break;
					}
				case 2: //Either ms/script or api/operation
					{
						var microserviceToken = hasBaseFile ? baseMs.Token : MicroService;
						var microService = hasBaseFile ? baseMs : ms;

						var component = Tenant.GetService<IComponentService>().SelectComponentByNameSpace(microserviceToken, ComponentCategories.NameSpacePublicScript, tokens[0]);

						//Check for exact match before returning
						if (IsOfCategory(component, ComponentCategories.Api))
						{
							var configuration = GetConfiguration<IApiConfiguration>(component.Token);

							if (configuration?.Operations.Any(e => string.Compare(e.Name, TrimExtension(tokens[1]), true) == 0) ?? false)
							{
								resolvedPath = $"{microService.Name}/{tokens[0]}/{tokens[1]}";
								break;
							}
						}
						else if (IsOfCategory(component, ComponentCategories.IoCContainer))
						{
							var configuration = GetConfiguration<IIoCContainerConfiguration>(component.Token);

							if (configuration?.Operations.Any(e => string.Compare(e.Name, TrimExtension(tokens[1]), true) == 0) ?? false)
							{
								resolvedPath = $"{microService.Name}/{tokens[0]}/{tokens[1]}";
								break;
							}
						}

						//No component exact match found, most likely ms/script

						//Trust, but verify
						if (Tenant.GetService<IMicroServiceService>().Select(tokens[0]) is null)
						{
							//Issue invalid path and let it fail silently down along the line to preserve compatibility
							resolvedPath = $"{microService.Name}/{tokens[0]}/{tokens[1]}";
							break;
						}

						//Microservice was found, assume ms/component combo
						resolvedPath = $"{tokens[0]}/{tokens[1]}";
						break;
					}
				case 1: //script
					{
						if (hasBaseFile)
							resolvedPath = $"{baseMs.Name}/{tokens[0]}";
						else
							resolvedPath = $"{ms.Name}/{tokens[0]}";
						break;
					}
			}

			return ValidatePath(ms, baseMs, resolvedPath);
		}

		private static string ValidatePath(IMicroService origin, IMicroService baseMicroService, string resolvedPath)
		{
			var target = baseMicroService ?? origin;

			var ms = resolvedPath.Split('/')[0];

			target.ValidateMicroServiceReference(ms);

			return resolvedPath;
		}

		private static string TrimExtension(string fileName) => fileName.EndsWith(".csx") ? fileName[0..^4] : fileName;

		private static bool IsOfCategory(IComponent component, string category) => string.Compare(component?.Category, category, true) == 0;

		private IConfiguration GetConfiguration(Guid token) => Tenant.GetService<IComponentService>().SelectConfiguration(token);

		private T GetConfiguration<T>(Guid token) => GetConfiguration(token) is T matching ? matching : default;
	}
}
