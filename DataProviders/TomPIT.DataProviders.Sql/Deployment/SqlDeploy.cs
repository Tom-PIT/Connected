using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TomPIT.Deployment;
using TomPIT.Deployment.Database;

namespace TomPIT.DataProviders.Sql.Deployment
{
	internal class SqlDeploy
	{
		public SqlDeploy(IDatabaseDeploymentContext context, IDatabase existing)
		{
			Context = context;
			Existing = existing;
			Command = new DeployCommand(context.ConnectionString);
		}

		private IDatabaseDeploymentContext Context { get; }
		private IDatabase Existing { get; }
		private DeployCommand Command { get; }
		private IDatabase LastState { get; set; }

		public void Deploy()
		{
			Command.Begin();

			try
			{
				LastState = Context.LoadState(Context);

				DropObsolete();
				DeploySchemas();
				DeployTables();
				DeployViews();
				DeployRoutines();
				Context.SaveState();

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
				if (Context.Database.Routines.Find(routine.Schema, routine.Name) == null)
					Command.DropProcedure(routine);
			}

			foreach (var view in LastState.Views)
			{
				if (Context.Database.Views.Find(view.Schema, view.Name) == null)
					Command.DropView(view);
			}

			foreach (var table in LastState.Tables)
			{
				var target = Context.Database.Tables.Find(table.Schema, table.Name);

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

			foreach (var table in Context.Database.Tables)
			{
				if (!schemaList.Contains(table.Schema))
					schemaList.Add(table.Schema);
			}

			foreach (var view in Context.Database.Views)
			{
				if (!schemaList.Contains(view.Schema))
					schemaList.Add(view.Schema);
			}

			foreach (var routine in Context.Database.Routines)
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
			foreach (var routine in Context.Database.Routines)
			{
				var existing = Existing.Routines.Find(routine.Schema, routine.Name);
				var code = routine.Definition;

				if (existing != null)
					code = ParseCommandText(routine, "ALTER");
				else
					code = ParseCommandText(routine, "CREATE");

				Command.Exec(code);
			}
		}

		private string ParseCommandText(IRoutine routine, string keyword)
		{
			var sb = new StringBuilder();
			var statementStarted = false;

			using (var s = new StringReader(routine.Definition))
			{
				while (s.Peek() != -1)
				{
					var line = s.ReadLine();

					if (statementStarted)
						sb.AppendLine(line);
					else
					{
						var tokens = line.Trim().Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

						if (tokens.Length == 0)
						{
							sb.AppendLine(line);
							continue;
						}

						if (tokens[0].StartsWith("CREATE", StringComparison.OrdinalIgnoreCase))
						{
							if (tokens.Length > 1)
							{
								if (tokens[1].StartsWith("PROCEDURE", StringComparison.OrdinalIgnoreCase))
								{
									statementStarted = true;
									sb.AppendLine($"{keyword} PROCEDURE [{routine.Schema}].[{routine.Name}]");
								}
								else if (tokens[1].StartsWith("FUNCTION", StringComparison.OrdinalIgnoreCase))
								{
									statementStarted = true;
									sb.AppendLine($"{keyword} FUNCTION [{routine.Schema}].[{routine.Name}]()");
								}
							}
							else
								sb.AppendLine(line);
						}
						else
							sb.AppendLine(line);
					}
				}
			}

			return sb.ToString();
		}

		private void DeployViews()
		{
			foreach (var view in Context.Database.Views)
			{
				var existing = Existing.Views.FirstOrDefault(f => string.Compare(view.Schema, f.Schema, true) == 0 && string.Compare(view.Name, f.Name, true) == 0);
				var code = view.Definition;

				if (existing != null)
					code = string.Format("ALTER {0}", code.Trim().Substring(6));

				Command.Exec(code);
			}
		}

		private void DeployTables()
		{
			foreach (var table in Context.Database.Tables)
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

			var processedConstraints = new List<string>();

			foreach (var column in table.Columns)
			{
				var existingColumn = existing.FindColumn(column.Name);

				SynchronizeColumn(table, column, existing, existingColumn, processedConstraints);
			}
		}

		private void SynchronizeColumn(ITable table, ITableColumn column, ITable existingTable, ITableColumn existing, List<string> processedConstraints)
		{
			if (existing != null)
			{
				if (string.Compare(column.DefaultValue, existing.DefaultValue, false) != 0)
				{
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
						if (processedConstraints.Contains(constraint.Name))
							continue;

						processedConstraints.Add(constraint.Name);

						var columns = ConstraintColumns(table, constraint.Name);

						if (HasConstraintChanged(existingTable, table, constraint.Name))
						{
							if (ConstraintExists(table, constraint.Name))
								Command.DropConstraint(table, constraint);

							Command.AddUniqueConstraint(table, columns, constraint);
						}
					}
				}
			}
			else
			{
				foreach (var constraint in column.Constraints)
				{
					if (string.Compare(constraint.Type, "PRIMARY KEY", true) == 0)
						Command.AddPrimaryKey(table, column, constraint);
					else if (string.Compare(constraint.Type, "UNIQUE", true) == 0)
					{
						if (processedConstraints.Contains(constraint.Name))
							continue;

						processedConstraints.Add(constraint.Name);

						Command.AddUniqueConstraint(table, ConstraintColumns(table, constraint.Name), constraint);
					}
				}
			}
		}

		private bool HasConstraintChanged(ITable existing, ITable table, string constraintName)
		{
			var existingColumns = ConstraintColumns(existing, constraintName);
			var columns = ConstraintColumns(table, constraintName);

			if (existingColumns.Count != columns.Count)
				return true;

			for (var i = 0; i < existingColumns.Count; i++)
			{
				if (string.Compare(existingColumns[i], columns[i], false) != 0)
					return true;
			}

			return false;
		}

		private bool ConstraintExists(ITable table, string constraintName)
		{
			foreach (var column in table.Columns)
			{
				if (column.Constraints.FirstOrDefault(f => string.Compare(f.Name, constraintName, false) == 0) != null)
					return true;
			}

			return false;
		}

		private List<string> ConstraintColumns(ITable table, string constraintName)
		{
			var result = new List<string>();

			foreach (var column in table.Columns)
			{
				foreach (var constraint in column.Constraints)
				{
					if (string.Compare(constraint.Name, constraintName, false) == 0)
						result.Add(column.Name);
				}
			}

			return result;
		}

		private void AlterColumn(ITable table, ITableColumn column, ITableColumn existing)
		{
			var builder = new StringBuilder();

			builder.AppendLine(string.Format("ALTER TABLE [{0}].[{1}]", table.Schema, table.Name));
			builder.AppendFormat("ALTER COLUMN {0}", Command.CreateColumnCommandText(column));
			builder.AppendLine(";");

			Command.Exec(builder.ToString());
		}

		private void AddColumn(ITable table, ITableColumn column)
		{
			Command.AddColumn(table, column);
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
				 || column.Ordinal != existing.Ordinal
				 || string.Compare(column.DefaultValue, existing.DefaultValue, false) != 0)
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
					var columns = ConstraintColumns(table, constraint.Name);

					builder.AppendLine(string.Format("CONSTRAINT [{0}] UNIQUE NONCLUSTERED", constraint.Name));
					builder.AppendLine("(");

					for (var column = 0; column < columns.Count; column++)
					{
						builder.AppendLine(string.Format("[{0}] ASC", columns[column]));

						if (columns.Count > 1 && column < columns.Count - 1)
							builder.AppendLine(",");
					}

					builder.AppendLine(")");

					if (i < uniqueConstraints.Count - 1 && uniqueConstraints.Count > 1)
						builder.AppendLine(",");
				}
			}

			builder.AppendLine(");");

			Command.Exec(builder.ToString());

			foreach (var column in defaults)
				Command.AddDefault(table, column);
		}

		private void CreateReferences()
		{
			foreach (var table in Context.Database.Tables)
			{
				var existingTable = Existing.Tables.FirstOrDefault(f => string.Compare(f.Schema, table.Schema, true) == 0 && string.Compare(f.Name, table.Name, true) == 0);

				foreach (var j in table.Columns)
				{
					if (string.IsNullOrWhiteSpace(j.Reference.Name))
						continue;

					var existingColumn = existingTable?.Columns.FirstOrDefault(f => string.Compare(f.Name, j.Name, true) == 0);

					if (existingColumn != null && string.Compare(j.Reference.Name, j.Reference.Name, true) == 0)
						continue;

					var primaryKeyColumn = Context.Database.FindPrimaryKeyColumn(j.Reference.ReferenceName);
					var primaryKeyTable = Context.Database.FindPrimaryKeyTable(j.Reference.ReferenceName);

					Command.CreateForeignKey(primaryKeyTable, primaryKeyColumn, table, j);
				}
			}
		}

		private void CreateIndexes()
		{
			foreach (var table in Context.Database.Tables)
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
