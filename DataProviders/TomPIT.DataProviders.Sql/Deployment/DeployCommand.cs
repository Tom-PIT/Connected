using System.Collections.Generic;
using System.Data;
using System.Text;
using Microsoft.Data.SqlClient;
using TomPIT.Data.Sql;
using TomPIT.Deployment.Database;

namespace TomPIT.DataProviders.Sql.Deployment
{
	internal class DeployCommand
	{
		private SqlCommand _command = null;
		private ReliableSqlConnection _con = null;
		private IDbTransaction _transaction = null;

		public DeployCommand(string connectionString)
		{
			ConnectionString = connectionString;
		}

		private string ConnectionString { get; }
		private ReliableSqlConnection Connection
		{
			get
			{
				if (_con == null)
					_con = new ReliableSqlConnection(ConnectionString, RetryPolicy.DefaultFixed, RetryPolicy.DefaultFixed);

				return _con;
			}
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

		public SqlTransaction Transaction { get { return _transaction as SqlTransaction; } }

		public void Begin()
		{
			Connection.Open();
			_transaction = Connection.BeginTransaction();
		}

		public void Commit()
		{
			_transaction.Commit();
		}

		public void Rollback()
		{
			_transaction.Rollback();
		}

		public void Close()
		{
			Connection.Close();
		}

		public bool SchemaExists(string schema)
		{
			Command.CommandText = string.Format("SELECT * FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{0}'", schema);
			var rdr = Command.ExecuteReader();

			var r = rdr.HasRows;

			rdr.Close();

			return r;
		}

		public void CreateForeignKey(ITable masterTable, ITableColumn master, ITable detailTable, ITableColumn detail)
		{
			var builder = new StringBuilder();

			builder.AppendLine(string.Format("ALTER TABLE [{0}].[{1}] WITH CHECK ADD CONSTRAINT [{2}] FOREIGN KEY ([{3}])",
				detailTable.Schema, detailTable.Name, detail.Reference.Name, detail.Name));

			builder.AppendLine(string.Format("REFERENCES [{0}].[{1}] ([{2}])", masterTable.Schema, masterTable.Name, master.Name));

			if (!string.IsNullOrEmpty(detail.Reference.UpdateRule))
				builder.AppendLine(string.Format("ON UPDATE {0}", detail.Reference.UpdateRule));

			if (!string.IsNullOrEmpty(detail.Reference.DeleteRule))
				builder.AppendLine(string.Format("ON DELETE {0}", detail.Reference.DeleteRule));

			Command.CommandText = builder.ToString();
			Command.CommandType = CommandType.Text;
			Command.Transaction = Transaction;

			Command.ExecuteNonQuery();

			builder = new StringBuilder();

			builder.AppendFormat("ALTER TABLE [{0}].[{1}] CHECK CONSTRAINT [{2}]", detailTable.Schema, detailTable.Name, detail.Reference.Name);

			Command.CommandText = builder.ToString();
			Command.ExecuteNonQuery();
		}

		public void CreateNonClusteredIndex(ITableIndex index, ITable table)
		{
			var builder = new StringBuilder();

			builder.AppendFormat("CREATE NONCLUSTERED INDEX [{0}] ON [{1}].[{2}]", index.Name, table.Schema, table.Name);
			builder.AppendLine("(");

			for (var k = 0; k < index.Columns.Count; k++)
			{
				var column = index.Columns[k];

				builder.Append(string.Format("[{0}] ASC", column));

				if (k < index.Columns.Count - 1)
					builder.AppendLine(",");
			}

			builder.AppendLine(")");

			Command.CommandText = builder.ToString();
			Command.ExecuteNonQuery();
		}

		public void DropProcedure(IRoutine routine)
		{
			Command.CommandText = string.Format("DROP PROCEDURE [{0}].[{1}]", routine.Schema, routine.Name);
			Command.ExecuteNonQuery();
		}

		public void DropView(IView view)
		{
			Command.CommandText = string.Format("DROP VIEW [{0}].[{1}]", view.Schema, view.Name);
			Command.ExecuteNonQuery();
		}

		public void DropDefault(ITable table, ITableColumn column)
		{
			Command.CommandText = string.Format("ALTER TABLE [{0}].[{1}] DROP CONSTRAINT [DF_{2}_{3}]", table.Schema, table.Name, ToValidName(table.Name), ToValidName(column.Name));
			Command.ExecuteNonQuery();
		}

		public void DropConstraint(ITable table, IReferentialConstraint constraint)
		{
			Command.CommandText = string.Format("ALTER TABLE [{0}].[{1}] DROP CONSTRAINT [{2}]", table.Schema, table.Name, constraint.Name);
			Command.ExecuteNonQuery();
		}

		public void DropConstraint(ITable table, ITableConstraint constraint)
		{
			Command.CommandText = string.Format("ALTER TABLE [{0}].[{1}] DROP CONSTRAINT [{2}]", table.Schema, table.Name, constraint.Name);
			Command.ExecuteNonQuery();
		}

		public void DropTable(ITable table)
		{
			Command.CommandText = string.Format("DROP TABLE [{0}].[{1}]", table.Schema, table.Name);
			Command.ExecuteNonQuery();
		}

		public void DropColumn(ITable table, ITableColumn column)
		{
			Command.CommandText = string.Format("ALTER TABLE [{0}].[{1}] DROP COLUMN [{2}]", table.Schema, table.Name, column.Name);
			Command.ExecuteNonQuery();
		}

		public void AddDefault(ITable table, ITableColumn column)
		{
			Command.CommandText = string.Format("ALTER TABLE [{0}].[{1}] ADD CONSTRAINT [DF_{1}_{2}] DEFAULT {3} FOR [{2}]", table.Schema, ToValidName(table.Name), ToValidName(column.Name), column.DefaultValue);
			Command.ExecuteNonQuery();
		}

		public void CreateSchema(string name)
		{
			Command.CommandText = string.Format("CREATE SCHEMA {0}", name);
			Command.ExecuteNonQuery();
		}

		public void Exec(string definition)
		{
			Command.CommandText = definition;
			Command.ExecuteNonQuery();
		}

		public void AddPrimaryKey(ITable table, ITableColumn column, ITableConstraint constraint)
		{
			var builder = new StringBuilder();

			builder.AppendLine(string.Format("ALTER TABLE [{0}].[{1}]", table.Schema, table.Name));
			builder.AppendLine(string.Format("ADD CONSTRAINT [{0}] PRIMARY KEY CLUSTERED", constraint.Name));
			builder.AppendLine("(");
			builder.AppendLine(string.Format("[{0}] ASC", column.Name));
			builder.AppendLine(")");

			Command.CommandText = builder.ToString();
			Command.ExecuteNonQuery();
		}

		public void AddUniqueConstraint(ITable table, List<string> columns, ITableConstraint constraint)
		{
			var builder = new StringBuilder();

			builder.AppendLine(string.Format("ALTER TABLE [{0}].[{1}]", table.Schema, table.Name));
			builder.AppendLine(string.Format("ADD CONSTRAINT [{0}] UNIQUE NONCLUSTERED", constraint.Name));
			builder.AppendLine("(");

			for (var i = 0; i < columns.Count; i++)
			{
				builder.AppendLine($"[{columns[i]}] ASC");

				if (columns.Count > 0 && i < columns.Count - 1)
					builder.AppendLine(",");
			}

			builder.AppendLine(")");

			Command.CommandText = builder.ToString();
			Command.ExecuteNonQuery();
		}

		public void AddColumn(ITable table, ITableColumn column)
		{
			var builder = new StringBuilder();

			builder.AppendLine(string.Format("ALTER TABLE [{0}].[{1}]", table.Schema, table.Name));
			builder.AppendFormat("ADD {0}", CreateColumnCommandText(column));

			if (!string.IsNullOrEmpty(column.DefaultValue))
				builder.AppendLine(string.Format("CONSTRAINT [DF_{0}_{1}] DEFAULT {2}", ToValidName(table.Name), ToValidName(column.Name), column.DefaultValue));

			builder.AppendLine(";");

			Command.CommandText = builder.ToString();
			Command.ExecuteNonQuery();
		}

		public string CreateColumnCommandText(ITableColumn column)
		{
			var builder = new StringBuilder();

			builder.AppendFormat("[{0}] {1} ", column.Name, CreateDataTypeMetaData(column));

			if (column.Identity)
				builder.Append("IDENTITY(1,1) ");

			if (column.IsNullable)
				builder.Append("NULL ");
			else
				builder.Append("NOT NULL ");

			return builder.ToString();
		}

		private string CreateDataTypeMetaData(ITableColumn column)
		{
			if (string.Compare(column.DataType, "binary", false) == 0)
				return string.Format("[{0}]({1})", column.DataType, column.CharacterMaximumLength);
			else if (string.Compare(column.DataType, "char", false) == 0)
				return string.Format("[{0}]({1})", column.DataType, column.CharacterMaximumLength);
			else if (string.Compare(column.DataType, "datetime2", false) == 0)
				return string.Format("[{0}]({1})", column.DataType, column.DateTimePrecision);
			else if (string.Compare(column.DataType, "datetimeoffset", false) == 0)
				return string.Format("[{0}]({1})", column.DataType, column.DateTimePrecision);
			else if (string.Compare(column.DataType, "decimal", false) == 0)
				return string.Format("[{0}]({1},{2})", column.DataType, column.NumericPrecision, column.NumericScale);
			else if (string.Compare(column.DataType, "nchar", false) == 0)
				return string.Format("[{0}]({1})", column.DataType, column.CharacterMaximumLength);
			else if (string.Compare(column.DataType, "numeric", false) == 0)
				return string.Format("[{0}]({1},{2})", column.DataType, column.NumericPrecision, column.NumericScale);
			else if (string.Compare(column.DataType, "nvarchar", false) == 0)
				return string.Format("[{0}]({1})", column.DataType, column.CharacterMaximumLength == -1 ? "MAX" : column.CharacterMaximumLength.ToString());
			else if (string.Compare(column.DataType, "time", false) == 0)
				return string.Format("[{0}]({1})", column.DataType, column.DateTimePrecision);
			else if (string.Compare(column.DataType, "varbinary", false) == 0)
				return string.Format("[{0}]({1})", column.DataType, column.CharacterMaximumLength == -1 ? "MAX" : column.CharacterMaximumLength.ToString());
			else if (string.Compare(column.DataType, "varchar", false) == 0)
				return string.Format("[{0}]({1})", column.DataType, column.CharacterMaximumLength == -1 ? "MAX" : column.CharacterMaximumLength.ToString());
			else
				return string.Format("[{0}]", column.DataType);
		}

		private string ToValidName(string identifier)
		{
			return identifier.Replace(' ', '_');
		}
	}
}
