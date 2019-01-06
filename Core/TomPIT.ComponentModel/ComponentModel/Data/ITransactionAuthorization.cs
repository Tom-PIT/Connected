using System;

namespace TomPIT.ComponentModel.Data
{
	interface ITransactionAuthorization
	{
		Guid Entity { get; set; }
		string KeyParameter { get; set; }

		IServerEvent Executing { get; }
	}
}
