namespace TomPIT.IoT.Services
{
	public interface IIoTHubService
	{
		IIoTDevice SelectDevice(string authenticationToken);
	}
}
