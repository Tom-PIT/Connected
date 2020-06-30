using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using TomPIT.Annotations.Models;
using TomPIT.Data;
using TomPIT.Data.Sql;
using TomPIT.DataProviders.Sql.Synchronization.Commands;

namespace TomPIT.DataProviders.Sql.Synchronization
{
	internal class Synchronizer : ISynchronizer
	{
		private ReliableSqlConnection _con = null;
		private IDbTransaction _transaction = null;
		private Dictionary<ConstraintNameType, List<string>> _constraintNames = null;
		private ExistingModel _existingModel = null;

		public Synchronizer(string connectionString, IModelSchema model, List<IModelOperationSchema> procedures)
		{
			ConnectionString = connectionString;
			Model = model;
			Procedures = procedures;
		}

		private List<IModelOperationSchema> Procedures { get; }
		private string ConnectionString { get; }
		public IModelSchema Model { get; }

		public ExistingModel ExistingModel
		{
			get { return _existingModel; }
			set
			{
				if (ExistingModel != value)
				{
					_existingModel = value;

					foreach (var index in _existingModel.Descriptor.Constraints)
					{
						switch (index.ConstraintType)
						{
							case ConstraintType.Default:
								AddConstraint(ConstraintNameType.Default, index.Name);
								break;
							case ConstraintType.PrimaryKey:
								AddConstraint(ConstraintNameType.PrimaryKey, index.Name);
								break;
							case ConstraintType.Unique:
								AddConstraint(ConstraintNameType.Index, index.Name);
								break;
						}
					}
				}
			}
		}

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

				throw;
			}
			finally
			{
				Close();
			}
		}

		private void Synchronize()
		{
			new SchemaSynchronize(this).Execute();

			if (string.IsNullOrWhiteSpace(Model.Type) || string.Compare(Model.Type, SchemaAttribute.SchemaTypeTable, true) == 0)
				new TableSynchronize(this).Execute();
			else if (string.Compare(Model.Type, SchemaAttribute.SchemaTypeView, true) == 0)
				new ViewSynchronize(this).Execute();
			else
				throw new NotSupportedException();

			foreach (var procedure in Procedures)
				new ProcedureSynchronize(this, procedure.Text).Execute();
		}

		public SqlCommand CreateCommand(string commandText)
		{
			var result = Connection.CreateCommand();

			result.CommandType = CommandType.Text;
			result.Transaction = Transaction;

			if (!string.IsNullOrWhiteSpace(commandText))
				result.CommandText = commandText;

			return result;
		}
		public SqlCommand CreateCommand()
		{
			return CreateCommand(null);
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

		private Dictionary<ConstraintNameType, List<string>> ConstraintNames
		{
			get
			{
				if (_constraintNames == null)
					_constraintNames = new Dictionary<ConstraintNameType, List<string>>();

				return _constraintNames;
			}
		}

		private void Begin()
		{
			Connection.Open();
			_transaction = Connection.BeginTransaction();
		}

		private void Commit()
		{
			if (_transaction != null)
				_transaction.Commit();
		}

		private void Rollback()
		{
			if (_transaction != null)
				_transaction.Rollback();
		}

		private void Close()
		{
			if (Connection.State == ConnectionState.Open)
				Connection.Close();
		}

		private void AddConstraint(ConstraintNameType type, string name)
		{
			if (!ConstraintNames.TryGetValue(type, out List<string> existing))
			{
				existing = new List<string>();

				ConstraintNames.Add(type, existing);
			}

			if (!ConstraintNameExists(name))
				existing.Add(name);
		}
		public string GenerateConstraintName(ConstraintNameType type)
		{
			var index = 0;

			while (true)
			{
				var value = $"{ConstraintPrefix(type)}_{Model.SchemaName().ToLowerInvariant()}_{Model.Name}";

				if (index > 0)
					value = $"{value}_{index}";

				if (!ConstraintNameExists(value))
				{
					AddConstraint(type, value);
					return value;
				}

				index++;
			}
		}

		private bool ConstraintNameExists(string value)
		{
			foreach (var key in ConstraintNames)
			{
				foreach (var item in key.Value)
				{
					if (item.Contains(value, StringComparison.OrdinalIgnoreCase))
						return true;
				}
			}

			return false;
		}

		private string ConstraintPrefix(ConstraintNameType type)
		{
			return type switch
			{
				ConstraintNameType.Default => "DF",
				ConstraintNameType.PrimaryKey => "PK",
				ConstraintNameType.Index => "IX",
				_ => "IX"
			};
		}
	}
}
