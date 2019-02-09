using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Events;
using TomPIT.ComponentModel.IoT;

namespace TomPIT.IoT
{
	[Create("IoTDevice", nameof(Name))]
	[ComponentCreatingHandler("TomPIT.IoT.Handlers.IoTDeviceCreateHandler, TomPIT.IoT.Design")]
	public class IoTDevice : ConfigurationElement, IIoTDevice
	{
		private IServerEvent _data = null;
		private ListItems<IIoTTransaction> _transactions = null;

		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[Required]
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		public string Name { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategorySecurity)]
		[Required]
		public string AuthenticationToken { get; set; }

		[EventArguments(typeof(IoTDataArguments))]
		public IServerEvent Data
		{
			get
			{
				if (_data == null)
					_data = new ServerEvent { Parent = this };

				return _data;
			}
		}

		[Items("TomPIT.IoT.Design.Items.IoTTransactionsCollection, TomPIT.IoT.Design")]
		public ListItems<IIoTTransaction> Transactions
		{
			get
			{
				if (_transactions == null)
					_transactions = new ListItems<IIoTTransaction> { Parent = this };

				return _transactions;
			}
		}

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				? base.ToString()
				: Name;
		}
	}
}
