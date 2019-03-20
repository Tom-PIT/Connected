namespace TomPIT.IoT
{
	public interface IIoTServiceNotification
	{
		void NotifyStateChanged(object sender, IoTStateChangedArgs e);
	}
}
