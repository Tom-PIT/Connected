using System;
using TomPIT.Development;

namespace TomPIT.Sys.Data
{
	internal class LockInfo : ILockInfo
	{
		public LockInfoResult Result { get; set; }
		public Guid Owner { get; set; }
	}
}
