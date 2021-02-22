using System.Net;

namespace TomPIT.Storage
{
	public interface IFileSystemCredentials : ICredentials
	{
		string Id { get; }
	}
}
