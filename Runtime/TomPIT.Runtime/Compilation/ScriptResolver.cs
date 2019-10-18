using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.ComponentModel.IoC;
using TomPIT.ComponentModel.Scripting;
using TomPIT.Connectivity;
using TomPIT.Exceptions;

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
			if (ReferenceEquals(null, obj))
				return false;

			if (ReferenceEquals(this, obj))
				return true;

			if (obj.GetType() != GetType())
				return false;

			return Equals((ScriptResolver)obj);
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
			return path;
		}

		public override Stream OpenRead(string resolvedPath)
		{
			var sourceCode = LoadScript(resolvedPath);

			if (sourceCode == null)
				return null;

			var content = Tenant.GetService<IComponentService>().SelectText(sourceCode.Configuration().MicroService(), sourceCode);

			if (string.IsNullOrWhiteSpace(content))
				return null;

			return new MemoryStream(Encoding.UTF8.GetBytes(content));
		}

		public IText LoadScript(string resolvedPath)
		{
			var tokens = resolvedPath.Split("/");
			/*
			 * possible references:
			 * --------------------
			 * - Microservice/PublicScript (2)
			 * - Microservice/Api/Operation (3)
			 * - Microservice/IoCContainer/Operation (3)
			 */

			if (tokens.Length == 2)
				return LoadScript(tokens[0], tokens[1]);
			else if (tokens.Length == 3)
				return Load(tokens[0], tokens[1], tokens[2]);
			else
				return null;
		}

		private IText Load(string microService, string componentName, string element)
		{
			IMicroService ms = null;

			if (!string.IsNullOrWhiteSpace(microService))
			{
				ms = Tenant.GetService<IMicroServiceService>().Select(microService);

				if (ms == null)
					throw new RuntimeException($"{SR.ErrMicroServiceNotFound} ({microService})");

				if (ms.Token != MicroService)
				{
					var originMicroService = Tenant.GetService<IMicroServiceService>().Select(MicroService);

					originMicroService.ValidateMicroServiceReference(ms.Name);
				}
			}
			else
				ms = Tenant.GetService<IMicroServiceService>().Select(MicroService);

			var component = Tenant.GetService<IComponentService>().SelectComponentByNameSpace(ms.Token, ComponentCategories.NameSpacePublicScript, componentName);

			if (component == null)
				return null;

			if (string.Compare(component.Category, ComponentCategories.Api, true) == 0)
				return LoadApiOperation(microService, component, element);
			else if (string.Compare(component.Category, ComponentCategories.IoCContainer, true) == 0)
				return LoadIoCOperation(microService, component, element);
			else
				return null;
		}

		private IText LoadApiOperation(string microService, IComponent component, string operation)
		{
			if (!(Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is IApiConfiguration config))
				throw new RuntimeException($"{SR.ErrServiceOperationNotFound} ({microService}/{component.Name}/{operation})");

			if (config.MicroService() != MicroService)
			{
				if (config.Scope != ElementScope.Public)
					throw new RuntimeException(SR.ErrScopeError);
			}

			var op = config.Operations.FirstOrDefault(f => string.Compare(f.Name, TrimExtension(operation), true) == 0);

			if (op == null)
				return null;

			if (config.MicroService() != MicroService)
			{
				if (op.Scope != ElementScope.Public)
					throw new RuntimeException(SR.ErrScopeError);
			}

			return op;
		}

		private IText LoadIoCOperation(string microService, IComponent component, string operation)
		{
			if (!(Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is IIoCContainerConfiguration config))
				throw new RuntimeException($"{SR.ErrIoCOperationNotFound} ({microService}/{component.Name}/{operation})");

			return config.Operations.FirstOrDefault(f => string.Compare(f.Name, TrimExtension(operation), true) == 0);
		}

		private IText LoadScript(string microService, string script)
		{
			IMicroService ms = null;

			ms = Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				throw new RuntimeException($"{SR.ErrMicroServiceNotFound} ({microService})");

			if (ms.Token != MicroService)
			{
				var originMicroService = Tenant.GetService<IMicroServiceService>().Select(MicroService);

				originMicroService.ValidateMicroServiceReference(ms.Name);
			}

			var scriptName = TrimExtension(script);
			var component = Tenant.GetService<IComponentService>().SelectComponentByNameSpace(ms.Token, ComponentCategories.NameSpacePublicScript, scriptName);

			if (component == null)
				return null;

			if (!(Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is IText text))
				throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrComponentNotFound, microService, scriptName));

			if (((IConfiguration)text).MicroService() != MicroService)
			{
				if (text is IScriptConfiguration s && s.Scope != ElementScope.Public)
					throw new RuntimeException(SR.ErrScopeError);
			}

			return text;
		}

		public override string ResolveReference(string path, string baseFilePath)
		{
			var resolvedPath = path;

			/*
			 * possible references:
			 * --------------------
			 * - Script (1)
			 * - Microservice/Script (2)
			 * - Api/Operation (2)
			 * - IoCContainer/Operation (2)
			 * - Microservice/Api/Operation (3)
			 * - Microservice/IoCContainer/Operation (3)
			 */

			var tokens = resolvedPath.Split(new char[] { '/' }, 3);
			var fileName = tokens[tokens.Length - 1];
			var extension = Path.GetExtension(fileName);
			var ms = Tenant.GetService<IMicroServiceService>().Select(MicroService);
			IMicroService baseMs = null;

			if (baseFilePath != null)
			{
				var baseMsToken = baseFilePath.Split('/')[0];

				if (baseMsToken.EndsWith(".csx"))
					baseMs = ms;
				else
				{
					baseMs = Tenant.GetService<IMicroServiceService>().Select(baseMsToken);

					if (baseMs == null)
						baseMs = ms;
				}
			}

			if (string.IsNullOrWhiteSpace(extension))
				tokens[^1] = $"{fileName}.csx";

			/*
			 * fully qualified
			 */
			if (tokens.Length == 3)
				return $"{tokens[0]}/{tokens[1]}/{tokens[2]}";
			else if (tokens.Length == 2)
			{
				if (string.IsNullOrWhiteSpace(baseFilePath))
				{
					var component = Tenant.GetService<IComponentService>().SelectComponentByNameSpace(MicroService, ComponentCategories.NameSpacePublicScript, tokens[0]);

					if (component != null &&
						(
							string.Compare(component.Category, ComponentCategories.Api, true) == 0
							|| string.Compare(component.Category, ComponentCategories.IoCContainer, true) == 0
						))
						return $"{ms.Name}/{tokens[0]}/{tokens[1]}"; //api with ms
					else
						return $"{tokens[0]}/{tokens[1]}";//script
				}
				else
				{
					var component = Tenant.GetService<IComponentService>().SelectComponentByNameSpace(baseMs.Token, ComponentCategories.NameSpacePublicScript, tokens[0]);

					if (component != null &&
						(
							string.Compare(component.Category, ComponentCategories.Api, true) == 0
							|| string.Compare(component.Category, ComponentCategories.IoCContainer, true) == 0
						))
						return $"{baseMs.Name}/{tokens[0]}/{tokens[1]}"; //api with basems
					else
						return $"{tokens[0]}/{tokens[1]}";//script
				}
			}
			else
			{
				/*
				 * script
				 */
				if (string.IsNullOrWhiteSpace(baseFilePath))
					return $"{ms.Name}/{tokens[0]}";
				else
					return $"{baseMs.Name}/{tokens[0]}";
			}
		}

		private string TrimExtension(string fileName)
		{
			return fileName.EndsWith(".csx") ? fileName[0..^4] : fileName;
		}
	}
}
