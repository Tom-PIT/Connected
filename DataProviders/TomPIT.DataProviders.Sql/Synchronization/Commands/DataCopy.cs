using System;
using System.Linq;
using System.Text;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class DataCopy : TableTransaction
	{
		public DataCopy(ISynchronizer owner, ExistingModel existing, string temporaryName) : base(owner)
		{
			Existing = existing;
			TemporaryName = temporaryName;
		}

		private ExistingModel Existing { get; }
		public string TemporaryName { get; }

		protected override void OnExecute()
		{
			Owner.CreateCommand(CommandText).ExecuteNonQuery();
		}

		private string CommandText
		{
			get
			{
				var text = new StringBuilder();
				var columnSet = new StringBuilder();
				var sourceSet = new StringBuilder();
				var comma = string.Empty;

				foreach (var column in Model.Columns)
				{
					if (column.IsVersion)
						continue;

					var existing = Existing.Columns.FirstOrDefault(f => string.Compare(column.Name, f.Name, true) == 0);

					if (existing == null)
						continue;

					columnSet.Append($"{comma}{Escape(column.Name)}");

					if (NeedsConversion(column) && (existing.DataType != column.DataType || existing.Precision != column.Precision || existing.Scale != column.Scale))
						sourceSet.Append($"{comma}CONVERT({ConversionString(column)},{Escape(column.Name)})");
					else
						sourceSet.Append($"{comma}{Escape(column.Name)}");

					comma = ",";
				}

				text.AppendLine($"IF EXISTS (SELECT * FROM {Escape(Existing.SchemaName(), Existing.Name)})");
				text.AppendLine($"EXEC ('INSERT INTO {Escape(Model.SchemaName(), TemporaryName)} ({columnSet.ToString()})");
				text.AppendLine($"SELECT {sourceSet.ToString()} FROM {Escape(Existing.SchemaName(), Existing.Name)}')");

				return text.ToString();
			}
		}

		private string ConversionString(IModelSchemaColumn column)
		{
			switch (column.DataType)
			{
				case System.Data.DbType.Byte:
					return "tinyint";
				case System.Data.DbType.Currency:
					return "money";
				case System.Data.DbType.Decimal:
					return $"decimal({column.Precision}, {column.Scale})";
				case System.Data.DbType.Double:
					return "real";
				case System.Data.DbType.Int16:
					return "smallint";
				case System.Data.DbType.Int32:
					return "int";
				case System.Data.DbType.Int64:
					return "bigint";
				case System.Data.DbType.SByte:
					return "smallint";
				case System.Data.DbType.Single:
					return "float";
				case System.Data.DbType.UInt16:
					return "int";
				case System.Data.DbType.UInt32:
					return "bigint";
				case System.Data.DbType.UInt64:
					return "float";
				case System.Data.DbType.VarNumeric:
					return $"numeric({column.Precision}, {column.Scale})";
				default:
					throw new NotSupportedException();
			}
		}

		private bool NeedsConversion(IModelSchemaColumn column)
		{
			switch (column.DataType)
			{
				case System.Data.DbType.Byte:
				case System.Data.DbType.Currency:
				case System.Data.DbType.Decimal:
				case System.Data.DbType.Double:
				case System.Data.DbType.Int16:
				case System.Data.DbType.Int32:
				case System.Data.DbType.Int64:
				case System.Data.DbType.SByte:
				case System.Data.DbType.Single:
				case System.Data.DbType.UInt16:
				case System.Data.DbType.UInt32:
				case System.Data.DbType.UInt64:
				case System.Data.DbType.VarNumeric:
					return true;
			}

			return false;
		}


	}
}
