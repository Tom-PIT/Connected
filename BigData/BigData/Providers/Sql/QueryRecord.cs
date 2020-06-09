using TomPIT.Data.Sql;

namespace TomPIT.BigData.Providers.Sql
{
	internal class QueryRecord : DatabaseRecord
	{
		public QueryField[] ItemArray { get; private set; }

		protected override void OnCreate()
		{
			ItemArray = new QueryField[Reader.FieldCount];

			for (int i = 0; i < ItemArray.Length; i++)
			{
				ItemArray[i] = new QueryField
				{
					Name = Reader.GetName(i),
					Value = Reader.GetValue(i)
				};
			}
		}
	}
}
