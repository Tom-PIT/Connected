using System.Threading;
using System.Threading.Tasks;
using TomPIT.Sys.Model;

namespace TomPIT.Sys.Services
{
	internal class EventsPersistence : PersistenceService
	{
		protected override async Task OnPersist(CancellationToken cancel)
		{
			await DataModel.Events.Flush();
		}
	}
}
