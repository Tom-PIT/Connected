using TomPIT.Annotations;
using TomPIT.ComponentModel;

namespace TomPIT.IoT
{
	public class Hub : ComponentConfiguration, IIoTHub
	{
		private ListItems<IIoTDevice> _devices = null;
		private ListItems<IIoTSchemaField> _schema = null;

		[Items("TomPIT.IoT.Items.IoTDevicesCollection, TomPIT.IoT.Design")]
		public ListItems<IIoTDevice> Devices
		{
			get
			{
				if (_devices == null)
					_devices = new ListItems<IIoTDevice> { Parent = this };

				return _devices;
			}
		}

		[Items("TomPIT.IoT.Items.IoTSchemaFieldsCollection, TomPIT.IoT.Design")]
		public ListItems<IIoTSchemaField> Schema
		{
			get
			{
				if (_schema == null)
					_schema = new ListItems<IIoTSchemaField> { Parent = this };

				return _schema;
			}
		}
	}
}
