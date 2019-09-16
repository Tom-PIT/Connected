using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;

namespace TomPIT.MicroServices.IoT
{
	public class Schema : ComponentConfiguration, IIoTSchemaConfiguration
	{
		private ListItems<IIoTTransaction> _transactions = null;
		private ListItems<IIoTSchemaField> _fields = null;

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

		[Items("TomPIT.MicroServices.IoT.Design.Items.IoTSchemaFieldsCollection, TomPIT.MicroServices.IoT.Design")]
		public ListItems<IIoTSchemaField> Fields
		{
			get
			{
				if (_fields == null)
					_fields = new ListItems<IIoTSchemaField>();

				return _fields;
			}
		}
	}
}
