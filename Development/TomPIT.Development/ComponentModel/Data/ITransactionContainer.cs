using Microsoft.Extensions.Configuration;

namespace TomPIT.ComponentModel.Data
{
	public interface ITransactionContainer : IConfiguration
	{
		ListItems<ITransaction> Transactions { get; }
	}
}
