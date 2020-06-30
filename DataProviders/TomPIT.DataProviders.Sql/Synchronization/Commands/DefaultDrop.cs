using System.Text;
using TomPIT.Data;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class DefaultDrop : ColumnTransaction
	{
		public DefaultDrop(ISynchronizer owner, IModelSchemaColumn column) : base(owner, column)
		{
		}

		protected override void OnExecute()
		{
			if (string.IsNullOrWhiteSpace(DefaultName))
				return;

			Owner.CreateCommand(CommandText).ExecuteNonQuery();
		}

		private string CommandText
		{
			get
			{
				var text = new StringBuilder();

				text.AppendLine($"ALTER TABLE {Escape(Model.SchemaName(), Model.Name)}");
				text.AppendLine($"DROP CONSTRAINT {DefaultName};");

				return text.ToString();
			}
		}

		private string DefaultName
		{
			get
			{
				if (Owner.ExistingModel == null)
					return null;

				foreach (var constraint in Owner.ExistingModel.Descriptor.Constraints)
				{
					if (constraint.ConstraintType == ConstraintType.Default)
					{
						if (constraint.Columns.Count == 1 && string.Compare(constraint.Columns[0], Column.Name, true) == 0)
							return constraint.Name;
					}
				}

				return null;
			}
		}
	}
}
