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
			//if (resolvedPath.StartsWith("$"))
			//	return LoadInternalScript(resolvedPath);

			var path = Path.GetFileNameWithoutExtension(resolvedPath);
			var tokens = path.Trim('/').Split('/');
			var lib = tokens[0];
			IMicroService ms = null;

			if (tokens.Length > 1)
			{
				ms = Connection.GetService<IMicroServiceService>().Select(tokens[0]);

				if (ms == null)
					throw new RuntimeException(string.Format("{0} ({1})", SR.ErrMicroServiceNotFound, tokens[0]));

				lib = tokens[1];
			}

			if (!(Connection.GetService<IComponentService>().SelectConfiguration(ms == null ? MicroService : ms.Token, "Library", lib) is ILibrary c))
				throw new RuntimeException(string.Format("{0} ({1})", SR.ErrComponentNotFound, lib));

			var sb = new StringBuilder();

			foreach (var i in c.Scripts)
			{
				var content = Connection.GetService<IComponentService>().SelectText(c.MicroService(Connection), i);

				if (!string.IsNullOrWhiteSpace(content))
					sb.AppendLine(content);
			}

			return new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
		}

		//private Stream LoadInternalScript(string qualifier)
		//{
		//	var tokens = qualifier.Substring(1).Split('/');

		//	var component = tokens[0].Trim();
		//	var script = tokens[1].Trim();

		//	if (!(Connection.GetService<IComponentService>().SelectConfiguration(component.AsGuid()) is ISourceCodeContainer container))
		//		throw new RuntimeException(SR.ErrSourceCodeContainerExected);

		//	var text = container.GetReference(script);
		//	var content = Connection.GetService<IComponentService>().SelectText(MicroService, text);

		//	return new MemoryStream(Encoding.UTF8.GetBytes(content));
		//}

		public override string ResolveReference(string path, string baseFilePath)
		{
			var extension = Path.GetExtension(path);

			if (string.IsNullOrWhiteSpace(extension))
				return string.Format("{0}.csx", path);

			return path;
		}
	}
}
