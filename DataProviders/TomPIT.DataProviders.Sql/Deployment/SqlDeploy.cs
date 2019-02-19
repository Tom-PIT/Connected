using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TomPIT.Connectivity;
using TomPIT.Deployment;
using TomPIT.Deployment.Database;
using TomPIT.Design.Serialization;
using TomPIT.Environment;
using TomPIT.Storage;

namespace TomPIT.DataProviders.Sql.Deployment
{
	internal class SqlDeploy
	{
		public SqlDeploy(ISysConnection connection, IPackage package, IDatabase database, IDatabase existing, string connectionString)
		{
			Package = package;
			Connection = connection;
			Database = database;
			Existing = existing;
			Command = new DeployCommand(connectionString);
		}

		private ISysConnection Connection { get; }
		private IDatabase Database { get; }
		private IDatabase Existing { get; }
		public DeployCommand Command { get; }
		private IDatabase LastState { get; set; }
		private IPackage Package { get; }
		private string StateKey => ((IPackageDatabase)Database).Connection.ToString();
		private Guid ResourceGroup => Connection.GetService<IResourceGroupService>().Default.Token;

		private void LoadState()
		{
			var blobs = Connection.GetService<IStorageService>().Query(Package.MicroService.Token, BlobTypes.DatabaseState, ResourceGroup, StateKey);

			if (blobs.Count != 0)
			{
				var content = Connection.GetService<IStorageService>().Download(blobs[0].Token);

				if (content != null)
					LastState = Connection.GetService<ISerializationService>().Deserialize(content.Content, typeof(Database)) as IDatabase;
			}
		}

		private void SaveState()
		{
			Connection.GetService<IStorageService>().Upload(new Blob
			{
				MicroService = Package.MicroService.Token,
				ContentType = "application/json",
				FileName = "databaseState.json",
				PrimaryKey = StateKey,
				ResourceGroup = ResourceGroup,
				Type = BlobTypes.DatabaseState
			}, Connection.GetService<ISerializationService>().Serialize(Database), StoragePolicy.Singleton);
		}

		public void Deploy()
		{
			Command.Begin();

			try
			{
				LoadState();
				DropObsolete();
				DeploySchemas();
				DeployTables();
				DeployViews();
				DeployRoutines();
				SaveState();

				Command.Commit();
			}
			catch
			{
				Command.Rollback();

				throw;
			}
			finally
			{
				Command.Close();
			}
		}

		private void DropObsolete()
		{
			if (LastState == null)
				return;

			foreach (var routine in LastState.Routines)
			{
				if (Database.Routines.Find(routine.Schema, routine.Name) == null)
					Command.DropProcedure(routine);
			}

			foreach (var view in LastState.Views)
			{
				if (Database.Views.Find(view.Schema, view.Name) == null)
					Command.DropView(view);
			}

			foreach (var table in LastState.Tables)
			{
				var target = Database.Tables.Find(table.Schema, table.Name);

				if (target == null)
				{
					foreach (var column in table.Columns)
					{
						foreach (var constraint in column.Constraints)
							Command.DropConstraint(table, constraint);

						if (!string.IsNullOrWhiteSpace(column.Reference.Name))
							Command.DropConstraint(table, column.Reference);
					}

					Command.DropTable(table);
				}
				else
				{
					foreach (var column in table.Columns)
					{
						var targetColumn = target.FindColumn(column.Name);

						if (targetColumn == null)
						{
							foreach (var constraint in column.Constraints)
								Command.DropConstraint(table, constraint);

							if (!string.IsNullOrWhiteSpace(column.Reference.Name))
								Command.DropConstraint(table, column.Reference);

							Command.DropColumn(table, column);
						}
						else
						{
							foreach (var constraint in column.Constraints)
							{
								var existing = targetColumn.Constraints.FirstOrDefault(f => string.Compare(constraint.Name, f.Name, true) == 0);

								if (existing == null)
									Command.DropConstraint(table, constraint);
							}

							if (!string.IsNullOrWhiteSpace(column.Reference.Name) && string.Compare(column.Reference.Name, targetColumn.Reference.Name) != 0)
								Command.DropConstraint(table, column.Reference);
						}
					}
				}
			}
		}

		private void DeploySchemas()
		{
			var schemaList = new List<string>();

			foreach (var table in Database.Tables)
			{
				if (!schemaList.Contains(table.Schema))
					schemaList.Add(table.Schema);
			}

			foreach (var view in Database.Views)
			{
				if (!schemaList.Contains(view.Schema))
					schemaList.Add(view.Schema);
			}

			foreach (var routine in Database.Routines)
			{
				if (!schemaList.Contains(routine.Schema))
					schemaList.Add(routine.Schema);
			}

			var targets = schemaList.Where(f => string.Compare(f, "dbo", true) != 0);

			if (targets.Count() == 0)
				return;

			foreach (var schema in targets)
			{
				if (Command.SchemaExists(schema))
					continue;

				Command.CreateSchema(schema);
			}
		}

		private void DeployRoutines()
		{
			foreach (var routine in Database.Routines)
			{
				var existing = Existing.Routines.Find(routine.Schema, routine.Name);
				var code = routine.Definition;

				if (existing != null)
				{
					var sb = new StringBuilder();
					var statementStarted = false;

					using (var s = new StringReader(code))
					{
						while (s.Peek() != -1)
						{
							var line = s.ReadLine();

							if (statementStarted)
								sb.AppendLine(line);
							else
							{
								if (line.Trim().StartsWith("CREATE PROCEDURE"))
								{
									statementStarted = true;
									sb.AppendLine(string.Format("ALTER {0}", line.Substring(6)));
								}
							}
						}
					}

					code = sb.ToString();
				}

				Command.Exec(code);
			}
		}

		private void DeployViews()
		{
			foreach (var view in Database.Views)
			{
				var existing = Existing.Views.FirstOrDefault(f => string.Compare(view.Schema, f.Schema, true) == 0 && string.Compare(view.Name, f.Name, true) == 0);
				var code = view.Definition;

				if (existing != null)
					code = string.Format("ALTER {0}", code.Substring(6));

				Command.Exec(code);
			}
		}

		private void DeployTables()
		{
			foreach (var table in Database.Tables)
				DeployTable(table);

			CreateIndexes();
			CreateReferences();
		}

		private void DeployTable(ITable table)
		{
			var existing = Existing.Tables.Find(table.Schema, table.Name);

			if (existing == null)
				CreateTable(table);
			else
				UpdateTable(table, existing);
		}

		private void UpdateTable(ITable table, ITable existing)
		{
			foreach (var column in table.Columns)
			{
				var existingColumn = existing.FindColumn(column.Name);

				if (existingColumn == null)
					AddColumn(table, column);
				else
				{
					if (!CompareColumns(column, existingColumn))
						AlterColumn(table, column, existingColumn);
				}
			}
		}

		private void AlterColumn(ITable table, ITableColumn column, ITableColumn existing)
		{
			var builder = new StringBuilder();

			builder.AppendLine(string.Format("ALTER TABLE [{0}].[{1}]", table.Schema, table.Name));
			builder.AppendFormat("ALTER COLUMN {0}", Command.CreateColumnCommandText(column));
			builder.AppendLine(";");

			Command.Exec(builder.ToString());

			if (string.Compare(column.DefaultValue, existing.DefaultValue, false) != 0)
			{
				if (!string.IsNullOrWhiteSpace(existing.DefaultValue))
					Command.DropDefault(table, column);

				Command.AddDefault(table, column);
			}

			foreach (var constraint in existing.Constraints)
			{
				if (column.Constraints.FirstOrDefault(f => string.Compare(f.Name, constraint.Name, true) == 0) != null)
					continue;

				Command.DropConstraint(table, constraint);
			}

			foreach (var constraint in column.Constraints)
			{
				if (string.Compare(constraint.Type, "PRIMARY KEY", true) == 0)
				{
					if (existing.Constraints.FirstOrDefault(f => string.Compare(f.Name, constraint.Name, true) == 0) != null)
						continue;

					Command.AddPrimaryKey(table, column, constraint);
				}
				else if (string.Compare(constraint.Type, "UNIQUE", true) == 0)
				{
					if (existing.Constraints.FirstOrDefault(f => string.Compare(f.Name, constraint.Name, true) == 0) != null)
						continue;

					Command.AddUniqueConstraint(table, column, constraint);
				}
			}
		}

		private void AddColumn(ITable table, ITableColumn column)
		{
			Command.AddColumn(table, column);

			foreach (var constraint in column.Constraints)
			{
				if (string.Compare(constraint.Type, "PRIMARY KEY", true) == 0)
					Command.AddPrimaryKey(table, column, constraint);
				else if (string.Compare(constraint.Type, "UNIQUE", true) == 0)
					Command.AddUniqueConstraint(table, column, constraint);
			}
		}

		private bool CompareColumns(ITableColumn column, ITableColumn existing)
		{
			if (column.CharacterMaximumLength != existing.CharacterMaximumLength
				|| column.CharacterOctetLength != existing.CharacterOctetLength
				|| column.CharacterSetName != existing.CharacterSetName
				|| column.CharacterSetName != existing.CharacterSetName
				|| column.DataType != existing.DataType
				|| column.DateTimePrecision != existing.DateTimePrecision
				|| column.Identity != existing.Identity
				|| column.IsNullable != existing.IsNullable
				|| column.NumericPrecision != existing.NumericPrecision
				|| column.NumericPrecisionRadix != existing.NumericPrecisionRadix
				|| column.NumericScale != existing.NumericScale
				|| column.Ordinal != existing.Ordinal)
				return false;

			return true;
		}

		private void CreateTable(ITable table)
		{
			var builder = new StringBuilder();

			builder.AppendLine(string.Format("CREATE TABLE [{0}].[{1}]", table.Schema, table.Name));
			builder.AppendLine("(");

			var primaryKey = table.ResolvePrimaryKey();
			var primaryKeyColumn = table.ResolvePrimaryKeyColumn();
			var uniqueConstraints = table.ResolveUniqueConstraints();
			var defaults = table.ResolveDefaults();

			for (var i = 0; i < table.Columns.Count; i++)
			{
				var column = table.Columns[i];

				builder.Append(Command.CreateColumnCommandText(column));

				if (i < table.Columns.Count - 1 || (primaryKey != null || uniqueConstraints.Count > 0))
					builder.AppendLine(",");
			}

			if (primaryKey != null)
			{
				builder.AppendLine(string.Format("CONSTRAINT [{0}] PRIMARY KEY CLUSTERED", primaryKey.Name));
				builder.AppendLine("(");
				builder.AppendLine(string.Format("[{0}] ASC", primaryKeyColumn.Name));
				builder.AppendLine(")");
			}

			if (uniqueConstraints.Count > 0)
			{
				if (primaryKey != null)
					builder.AppendLine(",");

				for (int i = 0; i < uniqueConstraints.Count; i++)
				{
					var constraint = uniqueConstraints[i];
					var column = Database.FindUniqueConstraintColumn(constraint.Name);

					builder.AppendLine(string.Format("CONSTRAINT [{0}] UNIQUE NONCLUSTERED", constraint.Name));
					builder.AppendLine("(");
					builder.AppendLine(string.Format("[{0}] ASC", column.Name));
					builder.AppendLine(")");

					if (i < uniqueConstraints.Count - 1)
						builder.AppendLine(")");
				}
			}

			builder.AppendLine(");");

			Command.Exec(builder.ToString());

			foreach (var column in defaults)
				Command.AddDefault(table, column);
		}

		private void CreateReferences()
		{
			foreach (var table in Database.Tables)
			{
				var existingTable = Existing.Tables.FirstOrDefault(f => string.Compare(f.Schema, table.Schema, true) == 0 && string.Compare(f.Name, table.Name, true) == 0);

				foreach (var j in table.Columns)
				{
					if (string.IsNullOrWhiteSpace(j.Reference.Name))
						continue;

					var existingColumn = existingTable?.Columns.FirstOrDefault(f => string.Compare(f.Name, j.Name, true) == 0);

					if (existingColumn != null && string.Compare(j.Reference.Name, j.Reference.Name, true) == 0)
						continue;

					var primaryKeyColumn = Database.FindPrimaryKeyColumn(j.Reference.ReferenceName);
					var primaryKeyTable = Database.FindPrimaryKeyTable(j.Reference.ReferenceName);

					Command.CreateForeignKey(primaryKeyTable, primaryKeyColumn, table, j);
				}
			}
		}

		private void CreateIndexes()
		{
			foreach (var table in Database.Tables)
			{
				var existingTable = Existing.Tables.FirstOrDefault(f => string.Compare(f.Schema, table.Schema, true) == 0 && string.Compare(f.Name, table.Name, true) == 0);

				foreach (var index in table.Indexes)
				{
					if (existingTable?.Indexes.FirstOrDefault(f => string.Compare(f.Name, index.Name, true) == 0) != null)
						continue;

					Command.CreateNonClusteredIndex(index, table);
				}
			}
		}
	}
}
