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

	public ISourceFileInfo Select(Guid token, int type)
	{
		return Connection.Post<SourceFileInfo>(Connection.CreateUrl(Controller, "Select"), new
		{
			token,
			type
		});
	}

	public void Upload(Guid microService, Guid configuration, Guid token, int type, string primaryKey, string fileName, string contentType, byte[] content, int version)
	{
		Connection.Post(Connection.CreateUrl(Controller, "Upload"), new
		{
			microService,
			configuration,
			token,
			type,
			primaryKey,
			fileName,
			contentType,
			content = Convert.ToBase64String(content ?? Array.Empty<byte>()),
			version
		});
	}

	public void BeginUpdate()
	{
		Connection.Post(Connection.CreateUrl(Controller, "BeginUpdate"));
	}

	public void EndUpdate()
	{
		Connection.Post(Connection.CreateUrl(Controller, "EndUpdate"));
	}
}
