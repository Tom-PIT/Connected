using System;
using TomPIT.Data;

namespace TomPIT.Api.Net
{
	public interface ISubscriber : ILongPrimaryKeyRecord
	{
		string Topic { get; }
		string Connection { get; }
		DateTime Alive { get; }
		DateTime Created { get; }
	}
}
