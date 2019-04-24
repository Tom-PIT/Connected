using Microsoft.CodeAnalysis;
using System;
using System.IO;
using System.Text;
using TomPIT.ComponentModel;
using TomPIT.Connectivity;

namespace TomPIT.Compilers
{
	public class ReferenceResolver : SourceReferenceResolver
	{
		public ReferenceResolver(ISysConnection connection, Guid microService)
		{
			Connection = connection;
			MicroService = microService;
		}

		private ISysConnection Connection { get; }
		private Guid MicroService { get; }

		protected bool Equals(ReferenceResolver other)
		{
			return Equals(Connection, other.Connection);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != GetType())
			{
				return false;
			}

			return Equals((ReferenceResolver)obj);
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
			var tokens = resolvedPath.Split("/");

			if (tokens.Length == 1)
				return LoadLibrary(null, resolvedPath);
			else if (tokens.Length == 2)
			{
				/*
				 * first try with internal library. if not found then we'll try to load
				 * public library from references microservice
				 */
				var component = Connection.GetService<IComponentService>().SelectComponent(MicroService, "Library", tokens[0]);

				if (component != null)
					return LoadScript(null, tokens[0], tokens[1]);
				else
					return LoadLibrary(tokens[0], tokens[1]);
			}
			else if (tokens.Length == 3)
				return LoadScript(tokens[0], tokens[1], tokens[2]);
			else
				return null;
		}

		private Stream LoadScript(string microService, string library, string script)
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

			if (!(Connection.GetService<IComponentService>().SelectConfiguration(ms.Token, "Library", library) is ISourceCodeContainer c))
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrComponentNotFound, library));

			if (c.MicroService(Connection) != MicroService)
			{
				if (c.Closest<ILibrary>().Scope != ElementScope.Public)
					throw new RuntimeException(SR.ErrScopeError);
			}

			var txt = c.GetReference(script);

			if (txt == null)
				return null;

			var content = Connection.GetService<IComponentService>().SelectText(c.MicroService(Connection), txt);

			if (string.IsNullOrWhiteSpace(content))
				return null;

			return new MemoryStream(Encoding.UTF8.GetBytes(content));
		}

		private Stream LoadLibrary(string microService, string library)
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

            if ((Connection.GetService<IComponentService>().SelectConfiguration(ms.Token, "Script", Path.GetFileNameWithoutExtension(library)) is IScript script))
            {
                if (((IConfiguration)script).MicroService(Connection) != MicroService)
                {
                    if (script.Scope != ElementScope.Public)
                        throw new RuntimeException(SR.ErrScopeError);
                }

                var content = Connection.GetService<IComponentService>().SelectText(((IConfiguration)script).MicroService(Connection), script);

                if (string.IsNullOrWhiteSpace(content))
                    return null;

                return new MemoryStream(Encoding.UTF8.GetBytes(content));
            }
            else
            {
                if (!(Connection.GetService<IComponentService>().SelectConfiguration(ms.Token, "Library", Path.GetFileNameWithoutExtension(library)) is ISourceCodeContainer container))
                    throw new RuntimeException(SR.ErrSourceCodeContainerExected);

                if (container.MicroService(Connection) != MicroService)
                {
                    if (container.Closest<ILibrary>().Scope != ElementScope.Public)
                        throw new RuntimeException(SR.ErrScopeError);
                }

                var refs = container.References();
                var sb = new StringBuilder();

                foreach (var i in refs)
                    sb.AppendLine($"#load \"{ms.Name}/{Path.GetFileNameWithoutExtension(library)}/{i}\"");

                return new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
            }
		}

		public override string ResolveReference(string path, string baseFilePath)
		{
			var extension = Path.GetExtension(path);

			if (string.IsNullOrWhiteSpace(extension))
				return string.Format("{0}.csx", path);

			return path;
		}
	}
}
