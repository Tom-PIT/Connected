﻿using System;

namespace TomPIT.Proxy;
public interface ISourceFileController
{
	void Delete(Guid microService, Guid token, int type);
	byte[] Download(Guid microService, Guid token, int type);
	void Upload(Guid microService, Guid token, int type, string primaryKey, string fileName, string contentType, byte[] content, int version);
}