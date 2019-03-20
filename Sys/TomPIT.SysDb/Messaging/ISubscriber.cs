using System;
using TomPIT.Data;

namespace TomPIT.SysDb.Messaging
{
	public interface ISubscriber : ILongPrimaryKeyRecord
	{
		string Topic { get; }
		string Connection { get; }
		DateTime Alive { get; }
		DateTime Created { get; }
		Guid Instance { get; }
	}
}
