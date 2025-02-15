﻿using System;
using System.Text.RegularExpressions;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class ProcedureSynchronize : SynchronizationTransaction
	{
		public ProcedureSynchronize(ISynchronizer owner, string text) : base(owner)
		{
			Text = text;
		}

		private string Text { get; }

		protected override void OnExecute()
		{
			var descriptor = new ProcedureTextParser().Parse(Text);

			if (descriptor.Type == Data.CommandTextType.Text)
				return;

			if (string.IsNullOrWhiteSpace(descriptor.Name))
				throw new RuntimeException(nameof(ProcedureSynchronize), SR.ErrCannotResolveProcedureName, LogCategories.Middleware);

			try
			{
				if (new ProcedureExists(Owner, descriptor.Name).Execute())
					AlterProcedure();
				else
					CreateProcedure();
			}
			catch (Exception ex)
			{
				throw new RuntimeException(descriptor.Name, ex.Message);
			}
		}

		private void CreateProcedure()
		{
			var command = Owner.CreateCommand();

			command.CommandText = ProcedureTextParser.ProcedureStatement.Replace(Text, SetCreateProcedureStatement);
			command.ExecuteNonQuery();
		}

		private void AlterProcedure()
		{
			var command = Owner.CreateCommand();

			command.CommandText = ProcedureTextParser.ProcedureStatement.Replace(Text, SetAlterProcedureStatement);
			command.ExecuteNonQuery();
		}

		private string SetAlterProcedureStatement(Match match)
		{
			if (match.Value.Trim().StartsWith("ALTER", System.StringComparison.OrdinalIgnoreCase))
				return match.Value;

			return $"ALTER {match.Value.Trim().Substring(7)}";
		}

		private string SetCreateProcedureStatement(Match match)
		{
			if (match.Value.Trim().StartsWith("CREATE", System.StringComparison.OrdinalIgnoreCase))
				return match.Value;

			return $"CREATE {match.Value.Trim().Substring(6)}";
		}
	}
}
