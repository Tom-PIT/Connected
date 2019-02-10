namespace TomPIT.IoT.Services
{
	internal interface IIoTHubNotification
	{
		void NotifyStateChanged(object sender, IoTStateChangedArgs e);
	}
}
