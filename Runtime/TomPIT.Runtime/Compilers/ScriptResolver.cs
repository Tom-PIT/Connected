using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Apis;
using TomPIT.Connectivity;

namespace TomPIT.Compilers
{
	public class ScriptResolver : SourceReferenceResolver
	{
		public ScriptResolver(ISysConnection connection, Guid microService)
		{
			Connection = connection;
			MicroService = microService;
		}

		private ISysConnection Connection { get; }
		private Guid MicroService { get; }

		protected bool Equals(ScriptResolver other)
		{
			return Equals(Connection, other.Connection);
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

				hashCode = (hashCode * 397) ^ (Connection?.GetHashCode() ?? 0);

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

			var content = Connection.GetService<IComponentService>().SelectText(sourceCode.MicroService(Connection), sourceCode);

			if (string.IsNullOrWhiteSpace(content))
				return null;

			return new MemoryStream(Encoding.UTF8.GetBytes(content));
		}

		public ISourceCode LoadScript(string resolvedPath)
		{
			var tokens = resolvedPath.Split("/");
			/*
			 * possible references:
			 * --------------------
			 * - Microservice/Script (2)
			 * - Microservice/Api/Operation (3)
			 */

			var ms = Connection.GetService<IMicroServiceService>().Select(tokens[0]);

			if (tokens.Length == 2)
				return LoadScript(tokens[0], tokens[1]);
			else if (tokens.Length == 3)
				return LoadApi(tokens[0], tokens[1], tokens[2]);
			else
				return null;
		}

		private ISourceCode LoadApi(string microService, string api, string operation)
		{
			IMicroService ms = null;

			if (!string.IsNullOrWhiteSpace(microService))
			{
				ms = Connection.GetService<IMicroServiceService>().Select(microService);

				if (ms == null)
					throw new RuntimeException($"{SR.ErrMicroServiceNotFound} ({microService})");

				if (ms.Token != MicroService)
				{
					var originMicroService = Connection.GetService<IMicroServiceService>().Select(MicroService);

					originMicroService.ValidateMicroServiceReference(Connection, ms.Name);
				}
			}
			else
				ms = Connection.GetService<IMicroServiceService>().Select(MicroService);

			var component = Connection.GetService<IComponentService>().SelectComponent(ms.Token, "Api", api);

			if (component == null)
				return null;

			if (!(Connection.GetService<IComponentService>().SelectConfiguration(component.Token) is IApi config))
				throw new RuntimeException($"{SR.ErrServiceOperationNotFound} ({microService}/{api}/{operation})");

			if (config.MicroService(Connection) != MicroService)
			{
				if (config.Scope != ElementScope.Public)
					throw new RuntimeException(SR.ErrScopeError);
			}
			
			var op = config.Operations.FirstOrDefault(f => string.Compare(f.Name, TrimExtension(operation), true) == 0);

			if (op == null)
				return null;// throw new RuntimeException($"{SR.ErrComponentNotFound} ({api}/{operation})");

			if (config.MicroService(Connection) != MicroService)
			{
				if (op.Scope != ElementScope.Public)
					throw new RuntimeException(SR.ErrScopeError);
			}

			return op;

		}

		private ISourceCode LoadScript(string microService, string script)
		{
			IMicroService ms = null;

			ms = Connection.GetService<IMicroServiceService>().Select(microService);

			if (ms == null)
				throw new RuntimeException($"{SR.ErrMicroServiceNotFound} ({microService})");

			if (ms.Token != MicroService)
			{
				var originMicroService = Connection.GetService<IMicroServiceService>().Select(MicroService);

				originMicroService.ValidateMicroServiceReference(Connection, ms.Name);
			}

			var scriptName = TrimExtension(script);
			var component = Connection.GetService<IComponentService>().SelectComponent(ms.Token, "Script", scriptName);

			if (component == null)
				return null;

			if (!(Connection.GetService<IComponentService>().SelectConfiguration(component.Token) is IScript s))
				throw new RuntimeException(string.Format("{0} ({1}/{2})", SR.ErrComponentNotFound, microService, scriptName));

			if (((IConfiguration)s).MicroService(Connection) != MicroService)
			{
				if (s.Scope != ElementScope.Public)
					throw new RuntimeException(SR.ErrScopeError);
			}

			return s;
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
			 * - Microservice/Api/Operation (3)
			 */

			var tokens = resolvedPath.Split(new char[] { '/' }, 3);
			var fileName = tokens[tokens.Length - 1];
			var extension = Path.GetExtension(fileName);
			var ms = Connection.GetService<IMicroServiceService>().Select(MicroService);
			IMicroService baseMs = null;

			if (baseFilePath != null)
			{ 
				var baseMsToken = baseFilePath.Split('/')[0];

				if (baseMsToken.EndsWith(".csx"))
					baseMs = ms;
				else
				{
					baseMs = Connection.GetService<IMicroServiceService>().Select(baseMsToken);

					if (baseMs == null)
						baseMs = ms;
				}
			}

			if (string.IsNullOrWhiteSpace(extension))
				tokens[tokens.Length - 1] = $"{fileName}.csx";

			/*
			 * fully qualified
			 */
			if(tokens.Length == 3)
				return $"{tokens[0]}/{tokens[1]}/{tokens[2]}";
			else if (tokens.Length == 2)
			{
				if(string.IsNullOrWhiteSpace(baseFilePath))
				{
					var api = Connection.GetService<IComponentService>().SelectComponent(MicroService, "Api", tokens[0]);

					if (api != null)
						return $"{ms.Name}/{tokens[0]}/{tokens[1]}"; //api with ms
					else
						return $"{tokens[0]}/{tokens[1]}";//script
				}
				else
				{
					var api = Connection.GetService<IComponentService>().SelectComponent(baseMs.Token, "Api", tokens[0]);
					
					if (api != null)
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
			return fileName.EndsWith(".csx") ? fileName.Substring(0, fileName.Length - 4) : fileName;
		}
	}
}
