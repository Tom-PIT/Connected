using System;
using System.Collections.Generic;
using TomPIT.Environment;
using TomPIT.Storage;

namespace TomPIT.Api.Storage;

public interface IBlobProvider
{
    IBlobContent Download(IServerResourceGroup resourceGroup, Guid blob);
    List<IBlobContent> Download(IServerResourceGroup resourceGroup, List<Guid> blobs);
    List<IBlobContent> Download(IServerResourceGroup resourceGroup, List<int> types);
    void Upload(IServerResourceGroup resourceGroup, Guid blob, byte[] content);
    void Delete(IServerResourceGroup resourceGroup, Guid blob);
}
