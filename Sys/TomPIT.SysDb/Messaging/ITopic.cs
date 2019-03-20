using System;
using TomPIT.Data;

namespace TomPIT.SysDb.Messaging
{
	public interface ITopic : ILongPrimaryKeyRecord
	{
		string Name { get; }
		Guid ResourceGroup { get; }
	}
}
