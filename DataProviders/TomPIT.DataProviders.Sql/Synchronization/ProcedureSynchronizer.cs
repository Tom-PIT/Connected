using System.Data.SqlClient;
using System.Text.RegularExpressions;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;

namespace TomPIT.DataProviders.Sql.Synchronization
{
	internal class ProcedureSynchronizer
	{
		public ProcedureSynchronizer(SqlCommand command, string text)
		{
			Command = command;
			Text = text;
		}

		private SqlCommand Command { get; }
		private string Text { get; }

		public void Execute()
		{
			var descriptor = new CommandTextParser().Parse(Text);

			if (descriptor.Type == Data.CommandTextType.Text)
				return;

			if (string.IsNullOrWhiteSpace(descriptor.Procedure))
				throw new RuntimeException(nameof(ProcedureSynchronizer), SR.ErrCannotResolveProcedureName, LogCategories.Middleware);

			if (ProcedureExists(descriptor.Procedure))
				AlterProcedure();
			else
				CreateProcedure();
		}

		private void CreateProcedure()
		{
			Command.CommandText = CommandTextParser.ProcedureStatement.Replace(Text, SetCreateProcedureStatement);
			Command.ExecuteNonQuery();
		}

		private void AlterProcedure()
		{
			Command.CommandText = CommandTextParser.ProcedureStatement.Replace(Text, SetAlterProcedureStatement);
			Command.ExecuteNonQuery();
		}

		private string SetAlterProcedureStatement(Match match)
		{
			if (match.Value.StartsWith("ALTER", System.StringComparison.OrdinalIgnoreCase))
				return match.Value;

			return $"ALTER {match.Value.Substring(7)}";
		}

		private string SetCreateProcedureStatement(Match match)
		{
			if (match.Value.StartsWith("CREATE", System.StringComparison.OrdinalIgnoreCase))
				return match.Value;

			return $"CREATE {match.Value.Substring(6)}";
		}

		private bool ProcedureExists(string name)
		{
			Command.CommandText = $"IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('{name}'))	SELECT 1; ELSE SELECT 0;";

			return Types.Convert<bool>(Command.ExecuteScalar());
		}
	}
}
