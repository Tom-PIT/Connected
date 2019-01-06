using System;
using TomPIT.Data;

namespace TomPIT.Api.Net
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
