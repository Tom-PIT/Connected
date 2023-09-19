using System.Threading;
using System.Threading.Tasks;

namespace TomPIT.Sys.Services
{
	internal class PrintingSpoolerPersistence : PersistenceService
	{
		protected override async Task OnPersist(CancellationToken cancel)
		{
			await Task.CompletedTask;// DataModel.Printing.Flush();
		}
	}
}
