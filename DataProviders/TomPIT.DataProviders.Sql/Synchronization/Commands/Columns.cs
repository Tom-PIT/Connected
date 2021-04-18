using System.Collections.Generic;
using System.Text;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class Columns : SynchronizationQuery<List<IModelSchemaColumn>>
	{
		public Columns(ISynchronizer owner, ExistingModel existing) : base(owner)
		{
			Existing = existing;
		}

		private ExistingModel Existing { get; }

		protected override List<IModelSchemaColumn> OnExecute()
		{
			var result = new List<IModelSchemaColumn>();
			var rdr = Owner.CreateCommand(CommandText).ExecuteReader();

			while (rdr.Read())
			{
				var column = new ExistingColumn(Existing)
				{
					IsNullable = string.Compare(rdr.GetValue("IS_NULLABLE", string.Empty), "NO", true) == 0 ? false : true,
					DataType = DataTypeUtils.ToDbType(rdr.GetValue("DATA_TYPE", string.Empty)),
					MaxLength = rdr.GetValue("CHARACTER_MAXIMUM_LENGTH", 0),
					Name = rdr.GetValue("COLUMN_NAME", string.Empty),
				};

				if (column.DataType == System.Data.DbType.Decimal || column.DataType == System.Data.DbType.VarNumeric)
				{
					column.Precision = rdr.GetValue("NUMERIC_PRECISION", 0);
					column.Scale = rdr.GetValue("NUMERIC_SCALE", 0);
				}

				if (column.DataType == System.Data.DbType.DateTime2
					|| column.DataType == System.Data.DbType.Time
					|| column.DataType == System.Data.DbType.DateTimeOffset)
					column.DatePrecision = rdr.GetValue("DATETIME_PRECISION", 0);

				if (column.DataType == System.Data.DbType.Date)
					column.DateKind = Annotations.Models.DateKind.Date;
				else if (column.DataType == System.Data.DbType.DateTime)
				{
					if (string.Compare(rdr.GetValue("DATA_TYPE", string.Empty), "smalldatetime", true) == 0)
						column.DateKind = Annotations.Models.DateKind.SmallDateTime;
				}
				else if (column.DataType == System.Data.DbType.DateTime2)
					column.DateKind = Annotations.Models.DateKind.DateTime2;
				else if (column.DataType == System.Data.DbType.Time)
					column.DateKind = Annotations.Models.DateKind.Time;
				else if (column.DataType == System.Data.DbType.Binary)
				{
					if (string.Compare(rdr.GetValue("DATA_TYPE", string.Empty), "varbinary", true) == 0)
						column.BinaryKind = Annotations.Models.BinaryKind.VarBinary;
					else if (string.Compare(rdr.GetValue("DATA_TYPE", string.Empty), "binary", true) == 0)
						column.BinaryKind = Annotations.Models.BinaryKind.Binary;
				}

				column.IsVersion = string.Compare(rdr.GetValue("DATA_TYPE", string.Empty), "timestamp", true) == 0;

				result.Add(column);
			}

			rdr.Close();

			return result;
		}

		private string CommandText
		{
			get
			{
				var text = new StringBuilder();

				text.AppendLine($"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = '{Model.SchemaName()}' AND TABLE_NAME = '{Owner.Model.Name}'");

				return text.ToString();
			}
		}
	}
}
