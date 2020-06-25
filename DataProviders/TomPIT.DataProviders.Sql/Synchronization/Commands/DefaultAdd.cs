using System.Text;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class DefaultAdd : ColumnTransaction
	{
		public DefaultAdd(ISynchronizer owner, IModelSchemaColumn column) : base(owner, column)
		{
		}

		protected override void OnExecute()
		{
			Owner.CreateCommand(CommandText).ExecuteNonQuery();
		}

		private string CommandText
		{
			get
			{
				var text = new StringBuilder();

				var defValue = $"N'{Column.DefaultValue}'";

				if (Column.DefaultValue.Length > 1)
				{
					var last = Column.DefaultValue.Trim()[^1];
					var prev = Column.DefaultValue.Trim()[0..^1].Trim()[^1];

					if (last == ')' && prev == '(')
						defValue = Column.DefaultValue;
				}

				text.AppendLine($"ALTER TABLE {Escape(Model.SchemaName(), Owner.Model.Name)}");
				text.AppendLine($"ADD CONSTRAINT DF_{Unescape(Model.SchemaName())}_{Unescape(Owner.Model.Name)}_{Column.Name} DEFAULT {defValue} FOR {Column.Name}");

				return text.ToString();
			}
		}
	}
}
