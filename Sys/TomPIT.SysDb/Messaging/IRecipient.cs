using System;
using TomPIT.Data;

namespace TomPIT.SysDb.Messaging
{
	public interface IRecipient : ILongPrimaryKeyRecord
	{
		string Connection { get; }
		Guid Message { get; }
		string Content { get; }
		string Topic { get; }
		int RetryCount { get; set; }
		DateTime NextVisible { get; set; }
	}
}
