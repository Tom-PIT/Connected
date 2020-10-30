using System;
using System.Text.RegularExpressions;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class ViewSynchronize : SynchronizationTransaction
	{
		public ViewSynchronize(ISynchronizer synchronizer, string text) : base(synchronizer)
		{
			Text = text;
		}

		private string Text { get; }

		protected override void OnExecute()
		{
			var descriptor = new ViewTextParser().Parse(Text);

			if (descriptor.Type != Data.CommandTextType.View)
				return;

			if (string.IsNullOrWhiteSpace(descriptor.Name))
				throw new RuntimeException(nameof(ViewSynchronize), SR.ErrCannotResolveViewName, LogCategories.Middleware);

			try
			{
				if (new ViewExists(Owner, descriptor.Name).Execute())
					AlterView();
				else
					CreateView();
			}
			catch (Exception ex)
			{
				throw new RuntimeException(descriptor.Name, ex.Message);
			}
		}

		private void CreateView()
		{
			var command = Owner.CreateCommand();

			command.CommandText = ViewTextParser.ViewStatement.Replace(Text, SetCreateViewStatement);
			command.ExecuteNonQuery();
		}

		private void AlterView()
		{
			var command = Owner.CreateCommand();

			command.CommandText = ViewTextParser.ViewStatement.Replace(Text, SetAlterViewStatement);
			command.ExecuteNonQuery();
		}

		private string SetAlterViewStatement(Match match)
		{
			if (match.Value.Trim().StartsWith("ALTER", StringComparison.OrdinalIgnoreCase))
				return match.Value;

			return $"ALTER {match.Value.Trim().Substring(7)}";
		}

		private string SetCreateViewStatement(Match match)
		{
			if (match.Value.Trim().StartsWith("CREATE", StringComparison.OrdinalIgnoreCase))
				return match.Value;

			return $"CREATE {match.Value.Trim().Substring(6)}";
		}
	}
}
