using System.Threading.Tasks;

namespace TomPIT.Cdn.Events
{
	/*
	 * Scaleout is currently not implemented on this service
	 */
	internal interface IEventHubService
	{
		Task NotifyAsync(EventHubNotificationArgs e);
	}
}
