﻿using TomPIT.Data.Sql;

namespace TomPIT.BigData.Providers.Sql
{
	internal class ExistingColumn : DatabaseRecord
	{
		public string ColumnName { get; set; }
		public string DataType { get; set; }
		public int TextLength { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			ColumnName = GetString("COLUMN_NAME");
			DataType = GetString("DATA_TYPE");
			TextLength = GetInt("CHARACTER_MAXIMUM_LENGTH");
		}
	}
}