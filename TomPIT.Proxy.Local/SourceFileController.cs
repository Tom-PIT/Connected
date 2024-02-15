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

	public void Upload(Guid microService, Guid token, int type, string primaryKey, string fileName, string contentType, byte[] content, int version)
	{
		DataModel.SourceFiles.Update(token, type, primaryKey, microService, fileName, contentType, version, content);
	}
}
