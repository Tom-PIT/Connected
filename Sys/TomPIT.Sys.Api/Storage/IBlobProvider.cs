using System;
using System.Collections.Generic;
using TomPIT.Storage;
using TomPIT.SysDb.Environment;

namespace TomPIT.Api.Storage
{
	public interface IBlobProvider
	{
		IBlobContent Download(IServerResourceGroup resourceGroup, Guid blob);
		List<IBlobContent> Download(IServerResourceGroup resourceGroup, List<Guid> blobs);

		void Upload(IServerResourceGroup resourceGroup, Guid blob, byte[] content);
		void Delete(IServerResourceGroup resourceGroup, Guid blob);
	}
}
