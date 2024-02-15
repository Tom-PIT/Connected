using System;

namespace TomPIT.ComponentModel;
internal sealed class SourceFileService : ISourceFileService
{
	public byte[] Load(Guid microService, Guid token)
	{
		throw new NotImplementedException();
	}

	public void Save(Guid microService, Guid token, string fileName, string contentType, string primaryKey, int type, int version, byte[] content)
	{
		throw new NotImplementedException();
	}

	public void Delete(Guid microService, Guid token)
	{

	}
}
