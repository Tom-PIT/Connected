using Amt.Api.Data;
using Amt.Sys.Model;

namespace Amt.DataHub.Transactions
{
	internal class MergeResultRecord : Record
	{
		public object[] ItemArray { get; private set; }

		protected override void OnCreate()
		{
			ItemArray = new object[Reader.FieldCount - 1];

			for (int i = 1; i < ItemArray.Length; i++)
				ItemArray[i - 1] = Reader[i];
		}
	}
}