using System;
using TomPIT.Data;

namespace TomPIT.SysDb.Messaging
{
	public interface IMessage : ILongPrimaryKeyRecord
	{
		string Text { get; }
		string Topic { get; }
		DateTime Created { get; }
		DateTime Expire { get; }
		TimeSpan RetryInterval { get; }
		Guid Token { get; }
	}
}
