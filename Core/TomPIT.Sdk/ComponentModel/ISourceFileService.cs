using System;

namespace TomPIT.ComponentModel;
public interface ISourceFileService
{
	byte[] Load(Guid microService, Guid token);
	void Save(Guid microService, Guid token, string fileName, string contentType, string primaryKey, int type, int version, byte[] content);
	void Delete(Guid microService, Guid token);
}
