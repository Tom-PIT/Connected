using System;
using TomPIT.Development;

namespace TomPIT.Ide.VersionControl
{
	internal class LockInfo : ILockInfo
	{
		public LockInfoResult Result { get; set; }
		public Guid Owner { get; set; }
	}
}
