using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
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
			if (path.Contains(":"))
				return path.Split(':')[1];

			return path;
		}

		public override Stream OpenRead(string resolvedPath)
		{
			var sourceCode = LoadScript(resolvedPath);

			if (sourceCode == null)
				return new MemoryStream(Encoding.UTF8.GetBytes(string.Empty));

			var content = Tenant.GetService<IComponentService>().SelectText(sourceCode.Configuration().MicroService(), sourceCode);

			if (string.IsNullOrWhiteSpace(content))
				return new MemoryStream(Encoding.UTF8.GetBytes(string.Empty));

			return new MemoryStream(Encoding.UTF8.GetBytes(content));
		}

		public IText LoadScript(string resolvedPath)
		{
			var root = resolvedPath.Split(':');
			var tokens = root.Length == 1 ? root[0].Split("/") : root[1].Split("/");
			var calling = root.Length > 1 ? root[0] : null;
			/*
			 * possible references:
			 * --------------------
			 * - Microservice/PublicScript (2)
			 * - Microservice/Api/Operation (3)
			 * - Microservice/AuthorizationPOlicy
			 */

			if (tokens.Length == 2)
				return LoadScript(calling, tokens[0], tokens[1]);
			else if (tokens.Length == 3)
				return Load(calling, tokens[0], tokens[1], tokens[2]);
			else
				return null;
		}

		private IText Load(string callingMicroService, string microService, string componentName, string element)
		{
			IMicroService ms = null;

			if (!string.IsNullOrWhiteSpace(microService))
			{
				ms = Tenant.GetService<IMicroServiceService>().Select(microService);

				if (ms == null)
					throw new RuntimeException($"{SR.ErrMicroServiceNotFound} ({microService})");
			}
			else
				ms = Tenant.GetService<IMicroServiceService>().Select(MicroService);

			var component = Tenant.GetService<IComponentService>().SelectComponentByNameSpace(ms.Token, ComponentCategories.NameSpacePublicScript, componentName);

			if (component == null)
				return null;

			if (string.Compare(component.Category, ComponentCategories.Api, true) == 0)
				return LoadApiOperation(microService, component, element);
			else
				return null;
		}

		private IText LoadApiOperation(string microService, IComponent component, string operation)
		{
			if (!(Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is IApiConfiguration config))
				throw new RuntimeException($"{SR.ErrServiceOperationNotFound} ({microService}/{component.Name}/{operation})");

			return config.Operations.FirstOrDefault(f => string.Compare(f.Name, TrimExtension(operation), true) == 0);
		}

		private IText LoadScript(string callingMicroService, string microService, string script)
		{
			var ms = Tenant.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				throw new RuntimeException($"{SR.ErrMicroServiceNotFound} ({microService})");

			var scriptName = TrimExtension(script);
			var component = Tenant.GetService<IComponentService>().SelectComponentByNameSpace(ms.Token, ComponentCategories.NameSpacePublicScript, scriptName);

			if (component == null || component.LockVerb == Development.LockVerb.Delete)
				return null;

			if (!(Tenant.GetService<IComponentService>().SelectConfiguration(component.Token) is IText text))
				throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrComponentNotFound, microService, scriptName));

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

			if (!string.IsNullOrWhiteSpace(baseFilePath))
			{
				var root = baseFilePath.Split(':');
				var baseMsToken = root.Length == 1
					? root[0].Split('/')[0]
					: root[1].Split('/')[0];

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
				return ValidatePath(ms, baseMs, $"{tokens[0]}/{tokens[1]}/{tokens[2]}");
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
						return ValidatePath(ms, baseMs, $"{ms.Name}/{tokens[0]}/{tokens[1]}"); //api with ms
					else
						return ValidatePath(ms, baseMs, $"{tokens[0]}/{tokens[1]}");//script
				}
				else
				{
					var component = Tenant.GetService<IComponentService>().SelectComponentByNameSpace(baseMs.Token, ComponentCategories.NameSpacePublicScript, tokens[0]);

					if (component != null &&
						(
							string.Compare(component.Category, ComponentCategories.Api, true) == 0
							|| string.Compare(component.Category, ComponentCategories.IoCContainer, true) == 0
						))
						return ValidatePath(ms, baseMs, $"{baseMs.Name}/{tokens[0]}/{tokens[1]}"); //api with basems
					else
						return ValidatePath(ms, baseMs, $"{tokens[0]}/{tokens[1]}");//script
				}
			}
			else
			{
				/*
				 * script
				 */
				if (string.IsNullOrWhiteSpace(baseFilePath))
					return ValidatePath(ms, baseMs, $"{ms.Name}/{tokens[0]}");
				else
					return ValidatePath(ms, baseMs, $"{baseMs.Name}/{tokens[0]}");
			}
		}

		private string ValidatePath(IMicroService origin, IMicroService baseMicroService, string resolvedPath)
		{
			var target = baseMicroService == null
				? origin
				: baseMicroService;

			var ms = resolvedPath.Split('/')[0];

			target.ValidateMicroServiceReference(ms);

			return resolvedPath;
		}

		private string TrimExtension(string fileName)
		{
			return fileName.EndsWith(".csx") ? fileName[0..^4] : fileName;
		}
	}
}
