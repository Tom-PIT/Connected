using System;

namespace TomPIT.Proxy.Remote;
internal sealed class SourceFileController : ISourceFileController
{
	private const string Controller = "SourceFile";
	public void Delete(Guid microService, Guid token, int type)
	{
		Connection.Post(Connection.CreateUrl(Controller, "Delete"), new
		{
			microService,
			token,
			type
		});
	}

	public byte[] Download(Guid microService, Guid token, int type)
	{
		return Connection.Post<byte[]>(Connection.CreateUrl(Controller, "Download"), new
		{
			microService,
			token,
			type
		});
	}

	public void Upload(Guid microService, Guid token, int type, string primaryKey, string fileName, string contentType, byte[] content, int version)
	{
		Connection.Post(Connection.CreateUrl(Controller, "Upload"), new
		{
			microService,
			token,
			type,
			primaryKey,
			fileName,
			contentType,
			content = Convert.ToBase64String(content ?? Array.Empty<byte>()),
			version
		});

	}
}
