using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Events;
using TomPIT.ComponentModel.IoT;

namespace TomPIT.IoT
{
	[Create("IoTDevice", nameof(Name))]
	[ComponentCreatingHandler("TomPIT.IoT.Handlers.IoTDeviceCreateHandler, TomPIT.IoT.Design")]
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
