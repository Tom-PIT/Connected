using System;
using TomPIT.Development;

namespace TomPIT.Proxy.Remote.Development;

internal class LockInfo : ILockInfo
{
    public LockInfoResult Result { get; set; }
    public Guid Owner { get; set; }
}
