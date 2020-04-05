using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;

namespace TomPIT.MicroServices.IoT
{
	[Create("IoTDevice", nameof(Name))]
	[ComponentCreatingHandler("TomPIT.MicroServices.IoT.Design.Handlers.IoTDeviceCreateHandler, TomPIT.MicroServices.IoT.Design")]
	[DomDesigner(DomDesignerAttribute.TextDesigner)]
	[Syntax(SyntaxAttribute.CSharp)]
	public class IoTDevice : ConfigurationElement, IIoTDevice
	{
		private ListItems<IIoTTransaction> _transactions = null;

		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		[Required]
		[InvalidateEnvironment(EnvironmentSection.Explorer | EnvironmentSection.Designer)]
		public string Name { get; set; }
		[PropertyCategory(PropertyCategoryAttribute.CategorySecurity)]
		[Required]
		public string AuthenticationToken { get; set; }

		[Items("TomPIT.MicroServices.IoT.Design.Items.IoTTransactionsCollection, TomPIT.MicroServices.IoT.Design")]
		public ListItems<IIoTTransaction> Transactions
		{
			get
			{
				if (_transactions == null)
					_transactions = new ListItems<IIoTTransaction> { Parent = this };

				return _transactions;
			}
		}

		[Browsable(false)]
		public Guid TextBlob { get; set; }

		public override string ToString()
		{
			return string.IsNullOrWhiteSpace(Name)
				? base.ToString()
				: Name;
		}
	}
}
