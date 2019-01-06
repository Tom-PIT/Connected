using System;
using TomPIT.Data;

namespace TomPIT.Api.Net
{
	public interface ITopic : ILongPrimaryKeyRecord
	{
		string Name { get; }
		Guid ResourceGroup { get; }
	}
}
