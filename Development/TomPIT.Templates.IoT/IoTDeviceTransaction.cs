using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;

namespace TomPIT.IoT
{
	public class IoTDeviceTransaction : Element, IIoTDeviceTransaction
	{
		[PropertyEditor(PropertyEditorAttribute.Select)]
		[Items("TomPIT.IoT.Design.Items.IoTDeviceTransactionsItems, TomPIT.IoT.Design")]
		public string Transaction { get; set; }
	}
}
