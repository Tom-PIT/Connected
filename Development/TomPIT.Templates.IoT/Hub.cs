using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;

namespace TomPIT.IoT
{
	public class Hub : ComponentConfiguration, IIoTHub
	{
		private ListItems<IIoTDevice> _devices = null;
		private ListItems<IIoTSchemaField> _schema = null;

		[Items("TomPIT.IoT.Design.Items.IoTDevicesCollection, TomPIT.IoT.Design")]
		public ListItems<IIoTDevice> Devices
		{
			get
			{
				if (_devices == null)
					_devices = new ListItems<IIoTDevice> { Parent = this };

				return _devices;
			}
		}

		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items("TomPIT.IoT.Design.Items.IotSchemasItems, TomPIT.IoT.Design")]
		public string Schema { get; set; }
		public ElementScope Scope { get; set; } = ElementScope.Internal;
	}
}
