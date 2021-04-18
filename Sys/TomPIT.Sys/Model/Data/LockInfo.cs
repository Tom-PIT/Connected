using System;
using TomPIT.Development;

namespace TomPIT.Sys.Model.Data
{
	internal class LockInfo : ILockInfo
	{
		public LockInfoResult Result { get; set; }
		public Guid Owner { get; set; }
	}
}
