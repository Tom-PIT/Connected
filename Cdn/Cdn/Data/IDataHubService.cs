using System.Threading.Tasks;

namespace TomPIT.Cdn.Data
{
	/*
	 * Scaleout is currently not implemented on this service
	 */
	internal interface IDataHubService
	{
		Task NotifyAsync(DataHubNotificationArgs e);
	}
}
