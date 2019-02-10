using TomPIT.IoT;

namespace TomPIT.Connectivity
{
	internal interface IIoTNotification
	{
		void NotifyChanged(object sender, IoTStateChangedArgs e);
	}
}
