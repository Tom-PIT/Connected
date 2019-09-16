using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;

namespace TomPIT.MicroServices.IoT
{
	public class Hub : ComponentConfiguration, IIoTHubConfiguration
	{
		private ListItems<IIoTDevice> _devices = null;

		[Items("TomPIT.MicroServices.IoT.Design.Items.IoTDevicesCollection, TomPIT.MicroServices.IoT.Design")]
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
		[Items("TomPIT.MicroServices.IoT.Design.Items.IoTSchemasItems, TomPIT.MicroServices.IoT.Design")]
		public string Schema { get; set; }
		public ElementScope Scope { get; set; } = ElementScope.Internal;
	}
}
