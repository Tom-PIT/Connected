using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.IoT;

namespace TomPIT.IoT
{
	public class Schema : ComponentConfiguration, IIoTSchema
	{
		private ListItems<IIoTTransaction> _transactions = null;
		private ListItems<IIoTSchemaField> _fields = null;

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

		[Items("TomPIT.IoT.Design.Items.IoTSchemaFieldsCollection, TomPIT.IoT.Design")]
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
