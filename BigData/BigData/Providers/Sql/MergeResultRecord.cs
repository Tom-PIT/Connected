using TomPIT.Data.Sql;

namespace TomPIT.BigData.Providers.Sql
{
	internal class MergeResultRecord : DatabaseRecord
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