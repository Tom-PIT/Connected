using System;
using Microsoft.Data.SqlClient;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class SpHelp : SynchronizationQuery<ObjectDescriptor>
	{
		private ObjectDescriptor _descriptor = null;
		public SpHelp(ISynchronizer owner) : base(owner)
		{

		}

		private ObjectDescriptor Result
		{
			get
			{
				if (_descriptor == null)
					_descriptor = new ObjectDescriptor();

				return _descriptor;
			}
		}

		protected override ObjectDescriptor OnExecute()
		{
			var command = Owner.CreateCommand();

			command.CommandType = System.Data.CommandType.StoredProcedure;
			command.CommandText = "sp_help";
			command.Parameters.AddWithValue("@objname", Escape(Model.SchemaName(), Owner.Model.Name));

			var rdr = command.ExecuteReader();

			try
			{
				ReadMetadata(rdr);
				ReadColumns(rdr);
				ReadIdentity(rdr);
				ReadRowGuid(rdr);
				ReadFileGroup(rdr);
				ReadIndexes(rdr);
				ReadConstraints(rdr);
			}
			finally
			{
				rdr.Close();
			}

			return Result;
		}

		private void ReadMetadata(SqlDataReader rdr)
		{
			if (rdr.Read())
			{
				Result.MetaData.Name = rdr.GetValue("Name", string.Empty);
				Result.MetaData.Created = rdr.GetValue("Created_datetime", DateTime.MinValue);
				Result.MetaData.Owner = rdr.GetValue("Owner", string.Empty);
				Result.MetaData.Type = rdr.GetValue("Type", string.Empty);
			}
		}

		private void ReadColumns(SqlDataReader rdr)
		{
			rdr.NextResult();

			while (rdr.Read())
			{
				Result.Columns.Add(new ObjectColumn
				{
					Collation = rdr.GetValue("Collation", string.Empty),
					Computed = string.Compare(rdr.GetValue("Computed", string.Empty), "no", true) == 0 ? false : true,
					FixedLenInSource = rdr.GetValue("FixedLenNullInSource", string.Empty),
					Length = rdr.GetValue("Length", 0),
					Name = rdr.GetValue("Column_name", string.Empty),
					Nullable = string.Compare(rdr.GetValue("Nullable", string.Empty), "no", true) == 0 ? false : true,
					Precision = Types.Convert<int>(rdr.GetValue("Prec", string.Empty).Trim()),
					Scale = Types.Convert<int>(rdr.GetValue("Scale", string.Empty).Trim()),
					TrimTrailingBlanks = rdr.GetValue("TrimTrailingBlanks", string.Empty),
					Type = rdr.GetValue("Type", string.Empty)
				});
			}
		}

		private void ReadIdentity(SqlDataReader rdr)
		{
			rdr.NextResult();

			if (rdr.Read())
			{
				Result.Identity.Identity = rdr.GetValue("Identity", string.Empty);
				Result.Identity.Increment = rdr.GetValue("Increment", 0);
				Result.Identity.NotForReplication = rdr.GetValue("Not For Replication", 0) == 0 ? false : true;
			}
		}

		private void ReadRowGuid(SqlDataReader rdr)
		{
			rdr.NextResult();

			if (rdr.Read())
				Result.RowGuid.RowGuidCol = rdr.GetValue("RowGuidCol", string.Empty);
		}

		private void ReadFileGroup(SqlDataReader rdr)
		{
			rdr.NextResult();

			if (rdr.Read())
				Result.FileGroup.FileGroup = rdr.GetValue("Data_located_on_filegroup", string.Empty);
		}

		private void ReadIndexes(SqlDataReader rdr)
		{
			rdr.NextResult();

			while (rdr.Read())
			{
				Result.Indexes.Add(new ObjectIndex
				{
					Description = rdr.GetValue("index_description", string.Empty),
					Keys = rdr.GetValue("index_keys", string.Empty),
					Name = rdr.GetValue("index_name", string.Empty)
				});
			}
		}

		private void ReadConstraints(SqlDataReader rdr)
		{
			rdr.NextResult();

			while (rdr.Read())
			{
				Result.Constraints.Add(new ObjectConstraint
				{
					DeleteAction = rdr.GetValue("delete_action", string.Empty),
					Keys = rdr.GetValue("constraint_keys", string.Empty),
					Name = rdr.GetValue("constraint_name", string.Empty),
					StatusEnabled = rdr.GetValue("status_enabled", string.Empty),
					StatusForReplication = rdr.GetValue("status_for_replication", string.Empty),
					Type = rdr.GetValue("constraint_type", string.Empty),
					UpdateAction = rdr.GetValue("update_action", string.Empty)
				});
			}
		}
	}
}
