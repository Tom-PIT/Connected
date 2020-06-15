using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using TomPIT.Data;
using TomPIT.Data.Sql;

namespace TomPIT.DataProviders.Sql.Synchronization
{
	internal class Synchronizer
	{
		private SqlCommand _command = null;
		private ReliableSqlConnection _con = null;
		private IDbTransaction _transaction = null;

		public Synchronizer(string connectionString, IModelSchema schema, List<IModelOperationSchema> procedures)
		{
			ConnectionString = connectionString;
			Schema = schema;
			Procedures = procedures;
		}

		private List<IModelOperationSchema> Procedures { get; }
		private string ConnectionString { get; }
		private IModelSchema Schema { get; }

		public void Execute()
		{
			try
			{
				Begin();
				Synchronize();
				Commit();
			}
			catch
			{
				Rollback();
			}
			finally
			{
				Close();
			}
		}

		private void Synchronize()
		{
			if (!SchemaExists())
				CreateSchema();

			if (string.IsNullOrWhiteSpace(Schema.Type) || string.Compare(Schema.Type, "Table", true) == 0)
				SynchronizeTable();
			else if (string.Compare(Schema.Type, "View", true) == 0)
				SynchronizeView();
			else
				throw new NotSupportedException();

			foreach (var procedure in Procedures)
				new ProcedureSynchronizer(Command, procedure.Text).Execute();
		}

		private SqlCommand Command
		{
			get
			{
				if (_command == null)
				{
					_command = Connection.CreateCommand();
					_command.CommandType = CommandType.Text;
					_command.Transaction = Transaction;
				}

				return _command;
			}
		}

		private SqlTransaction Transaction => _transaction as SqlTransaction;

		private ReliableSqlConnection Connection
		{
			get
			{
				if (_con == null)
					_con = new ReliableSqlConnection(ConnectionString, RetryPolicy.DefaultFixed, RetryPolicy.DefaultFixed);

				return _con;
			}
		}

		private void Begin()
		{
			Connection.Open();
			_transaction = Connection.BeginTransaction();
		}

		private void Commit()
		{
			_transaction.Commit();
		}

		private void Rollback()
		{
			_transaction.Rollback();
		}

		private void Close()
		{
			Connection.Close();
		}

		private string TableSchema => string.IsNullOrWhiteSpace(Schema.Schema) ? "dbo" : Schema.Schema;

		private bool SchemaExists()
		{
			if (string.IsNullOrWhiteSpace(Schema.Schema))
				return true;

			Command.CommandText = string.Format("SELECT * FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{0}'", Schema.Schema);

			var rdr = Command.ExecuteReader();

			var r = rdr.HasRows;

			rdr.Close();

			return r;
		}

		private void CreateSchema()
		{
			Command.CommandText = $"CREATE SCHEMA {TableSchema}";
			Command.ExecuteNonQuery();
		}

		private bool TableExists()
		{
			Command.CommandText = $"IF (EXISTS (SELECT *  FROM INFORMATION_SCHEMA.TABLES  WHERE TABLE_SCHEMA = '{TableSchema}'  AND  TABLE_NAME = '{Schema.Name}')) SELECT 1 ELSE SELECT 0";

			return Types.Convert<bool>(Command.ExecuteScalar());
		}

		private void SynchronizeView()
		{
			throw new NotImplementedException();
		}

		private void SynchronizeTable()
		{
			if (TableExists())
				AlterTable();
			else
				CreateTable();
		}

		private void CreateTable()
		{
			var text = new StringBuilder();

			text.AppendLine($"CREATE TABLE [{TableSchema}].[{Schema.Name}]");
			text.AppendLine("(");

			var primaryKey = Schema.Columns.FirstOrDefault(f => f.IsPrimaryKey);
			var unique = Schema.Columns.Where(f => f.IsUnique).ToList();

			for (var i = 0; i < Schema.Columns.Count; i++)
			{
				var column = Schema.Columns[i];

				text.Append(CreateColumnCommandText(column));

				if (i < Schema.Columns.Count - 1 || (primaryKey != null || unique.Count > 0))
					text.AppendLine(",");
			}

			if (primaryKey != null)
			{
				text.AppendLine($"CONSTRAINT [pk_{primaryKey.Name}] PRIMARY KEY CLUSTERED");
				text.AppendLine("(");
				text.AppendLine($"[{primaryKey.Name}] ASC");
				text.AppendLine(")");
			}

			text.AppendLine(");");

			Command.CommandText = text.ToString();
			Command.ExecuteNonQuery();

			//defaults
		}

		private void AlterTable()
		{

		}

		private string CreateColumnCommandText(IModelSchemaColumn column)
		{
			var builder = new StringBuilder();

			builder.AppendFormat($"[{column.Name}] {CreateDataTypeMetaData(column)} ");

			if (column.IsIdentity)
				builder.Append("IDENTITY(1,1) ");

			if (column.IsNullable)
				builder.Append("NULL ");
			else
				builder.Append("NOT NULL ");

			return builder.ToString();
		}

		private string CreateDataTypeMetaData(IModelSchemaColumn column)
		{
			return column.DataType switch
			{
				DbType.AnsiString => $"[varchar]({ResolveColumnLength(column)}",
				DbType.Binary => $"[binary]({ResolveColumnLength(column)}",
				DbType.Byte => "[tinyint]",
				DbType.Boolean => "[bit]",
				DbType.Currency => "[money]",
				DbType.Date => "[date]",
				DbType.DateTime => "[datetime]",
				DbType.Decimal => "[float]",
				DbType.Double => "[float]",
				DbType.Guid => "[uniqueidentifier]",
				DbType.Int16 => "[smallint]",
				DbType.Int32 => "[int]",
				DbType.Int64 => "[bigint]",
				DbType.Object => $"[varbinary]({ResolveColumnLength(column)})",
				DbType.SByte => "[smallint]",
				DbType.Single => "[float]",
				DbType.String => $"[nvarchar]({ResolveColumnLength(column)})",
				DbType.Time => $"[time]({ResolveColumnLength(column)})",
				DbType.UInt16 => "[int]",
				DbType.UInt32 => "[bigint]",
				DbType.UInt64 => "[float]",
				DbType.VarNumeric => "[float]",
				DbType.AnsiStringFixedLength => $"[char]({ResolveColumnLength(column)})",
				DbType.StringFixedLength => $"[char]({ResolveColumnLength(column)})",
				DbType.Xml => "[xml]",
				DbType.DateTime2 => $"[datetime2]({ResolveColumnLength(column)})",
				DbType.DateTimeOffset => $"[datetimeoffset]({ResolveColumnLength(column)})",
				_ => throw new NotSupportedException(),
			};
		}

		private string ResolveColumnLength(IModelSchemaColumn column)
		{
			if (column.MaxLength == -1)
				return "MAX";

			if (column.MaxLength > 0)
				return column.MaxLength.ToString();

			switch (column.DataType)
			{
				case DbType.AnsiString:
				case DbType.String:
				case DbType.Date:
				case DbType.DateTime:
				case DbType.AnsiStringFixedLength:
				case DbType.StringFixedLength:
					return 50.ToString();
				case DbType.Binary:
					return 128.ToString();
				case DbType.Time:
				case DbType.DateTime2:
				case DbType.DateTimeOffset:
					return 7.ToString();
				case DbType.VarNumeric:
					return 8.ToString();
				case DbType.Xml:
					return "MAX";
				default:
					return 50.ToString();
			}
		}
	}
}
