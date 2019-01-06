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
			var tokens = resolvedPath.Trim('/').Split('/');
			var lib = tokens[0];
			IMicroService ms = null;

			if (tokens.Length > 1)
			{
				ms = Connection.GetService<IMicroServiceService>().Select(tokens[0]);

				if (ms == null)
				{
					throw new RuntimeException(string.Format("{0} ({1})", SR.ErrMicroServiceNotFound, tokens[0]));
				}

				lib = tokens[1];
			}

			var c = Connection.GetService<IComponentService>().SelectConfiguration(ms == null ? MicroService : ms.Token, "Library", lib) as ILibrary;

			if (c == null)
			{
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrComponentNotFound, lib));
			}

			var sb = new StringBuilder();

			foreach (var i in c.Scripts)
			{
				var content = Connection.GetService<IComponentService>().SelectText(c.MicroService(Connection), i);

				if (!string.IsNullOrWhiteSpace(content))
				{
					sb.AppendLine(content);
				}
			}

			return new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
		}

		public override string ResolveReference(string path, string baseFilePath)
		{
			if (path.EndsWith(".csx"))
			{
				return path.Substring(0, path.Length - 4);
			}

			return path;
		}
	}
}
