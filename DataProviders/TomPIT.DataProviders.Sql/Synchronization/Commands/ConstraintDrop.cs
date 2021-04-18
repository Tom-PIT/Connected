using System.Text;

namespace TomPIT.DataProviders.Sql.Synchronization.Commands
{
	internal class ConstraintDrop : TableTransaction
	{
		public ConstraintDrop(ISynchronizer owner, ObjectIndex index) : base(owner)
		{
			Index = index;
		}

		private ObjectIndex Index { get; }

		protected override void OnExecute()
		{
			Owner.CreateCommand(CommandText).ExecuteNonQuery();
		}
		private string CommandText
		{
			get
			{
				var text = new StringBuilder();

				text.AppendLine($"ALTER TABLE {Escape(Model.Schema, Model.Name)} DROP CONSTRAINT {Index.Name};");

				return text.ToString();
			}
		}
	}
}
