using System.Linq;
using System.Text;

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
				var comma = string.Empty;

				foreach (var column in Model.Columns)
				{
					if (Existing.Columns.FirstOrDefault(f => string.Compare(column.Name, f.Name, true) == 0) == null)
						continue;

					columnSet.Append($"{comma}{column.Name}");
					comma = ",";
				}

				text.AppendLine($"IF EXISTS (SELECT * FROM {Escape(Existing.SchemaName(), Existing.Name)})");
				text.AppendLine($"EXEC ('INSERT INTO {Escape(Model.SchemaName(), TemporaryName)} ({columnSet.ToString()})");
				text.AppendLine($"SELECT {columnSet.ToString()} FROM {Escape(Existing.SchemaName(), Existing.Name)}')");

				return text.ToString();
			}
		}
	}
}
