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
			if (resolvedPath.Contains('/'))
				return LoadScript(resolvedPath);
			else
				return LoadLibrary(resolvedPath);
		}

		private Stream LoadScript(string qualifier)
		{
			var tokens = qualifier.Split('/');
			var lib = tokens[0];

			if (!(Connection.GetService<IComponentService>().SelectConfiguration(MicroService, "Library", lib) is ISourceCodeContainer c))
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrComponentNotFound, lib));

			var txt = c.GetReference(tokens[1]);

			if (txt == null)
				return null;

			var content = Connection.GetService<IComponentService>().SelectText(c.MicroService(Connection), txt);

			if (string.IsNullOrWhiteSpace(content))
				return null;

			return new MemoryStream(Encoding.UTF8.GetBytes(content));
		}

		private Stream LoadLibrary(string qualifier)
		{
			if (!(Connection.GetService<IComponentService>().SelectConfiguration(MicroService, "Library", Path.GetFileNameWithoutExtension(qualifier)) is ISourceCodeContainer container))
				throw new RuntimeException(SR.ErrSourceCodeContainerExected);

			var refs = container.References();
			var sb = new StringBuilder();

			foreach (var i in refs)
				sb.AppendLine(string.Format("#load \"{0}/{1}\"", qualifier, i));

			return new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
		}

		public override string ResolveReference(string path, string baseFilePath)
		{
			if (path.Contains('/'))
			{
				var tokens = path.Split('/');

				path = string.Format("{0}/{1}", Path.GetFileNameWithoutExtension(tokens[0]), tokens[1]);
			}

			var extension = Path.GetExtension(path);

			if (string.IsNullOrWhiteSpace(extension))
				return string.Format("{0}.csx", path);

			return path;
		}
	}
}
