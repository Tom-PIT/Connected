using Amt.Api.Data;
using Amt.Sys.Model;

namespace Amt.DataHub.Data
{
	internal class DataHubQueryRecord : Record
	{
		public object[] ItemArray { get; private set; }

		protected override void OnCreate()
		{
			ItemArray = new object[Reader.FieldCount];

			for (int i = 0; i < ItemArray.Length; i++)
				ItemArray[i] = Reader[i];
		}
	}
}
