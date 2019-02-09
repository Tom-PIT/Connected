using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Events;
using TomPIT.ComponentModel.IoT;

namespace TomPIT.IoT
{
	[Create("Transaction", nameof(Name))]
	public class IoTTransaction : ConfigurationElement, IIoTTransaction
	{
		private IServerEvent _invoke = null;
		private ListItems<IIoTTransactionParameter> _parameters = null;

		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		[Required]
		public string Name { get; set; }

		[EventArguments(typeof(IoTTransactionInvokeArguments))]
		public IServerEvent Invoke
		{
			get
			{
				if (_invoke == null)
					_invoke = new ServerEvent { Parent = this };

				return _invoke;
			}
		}

		[Items("TomPIT.IoT.Design.Items.IoTTransactionParametersCollection, TomPIT.IoT.Design")]
		public ListItems<IIoTTransactionParameter> Parameters
		{
			get
			{
				if (_parameters == null)
					_parameters = new ListItems<IIoTTransactionParameter> { Parent = this };

				return _parameters;
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
