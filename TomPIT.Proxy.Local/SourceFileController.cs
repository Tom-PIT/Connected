using System;
using TomPIT.Sys.Model;

namespace TomPIT.Proxy.Local;
internal sealed class SourceFileController : ISourceFileController
{
	public void Delete(Guid microService, Guid token, int type)
	{
		DataModel.SourceFiles.Delete(microService, token, type);
	}

	public byte[] Download(Guid microService, Guid token, int type)
	{
		return DataModel.SourceFiles.Select(microService, token, type);
	}

	public ISourceFileInfo Select(Guid token, int type)
	{
		var result = DataModel.SourceFiles.SelectFileInfo(token, type);

		if (result is null)
			return null;

		return new SourceFileInfo
		{
			ContentType = result.ContentType,
			FileName = result.FileName,
			MicroService = result.MicroService,
			Token = result.Token,
			Type = result.Type,
			Modified = result.Modified,
			PrimaryKey = result.PrimaryKey,
			Size = result.Size,
			Version = result.Version
		};
	}

	public void Upload(Guid microService, Guid token, int type, string primaryKey, string fileName, string contentType, byte[] content, int version)
	{
		DataModel.SourceFiles.Update(token, type, primaryKey, microService, fileName, contentType, version, content);
	}
}
