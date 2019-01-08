using TomPIT.Application.Data;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Application.Design.Dom
{
	internal class DataElement : Element
	{
		public const string ElementId = "Data";

		public DataElement(IEnvironment environment, IDomElement parent) : base(environment, parent)
		{
			Id = ElementId;
			Glyph = "fal fa-database";
			Title = "Data";
		}

		public override bool HasChildren => true;

		public override void LoadChildren()
		{
			LoadDataSources();
			LoadTransactions();
		}

		public override void LoadChildren(string id)
		{
			if (string.Compare(id, "DataSource", true) == 0)
				LoadDataSources();

			if (string.Compare(id, "Transactions", true) == 0)
				LoadTransactions();
		}

		private void LoadDataSources()
		{
			Items.Add(new CategoryElement(Environment, this, DataSource.ComponentCategory, "DataSource", "Data sources", "fal fa-database"));
		}
		private void LoadTransactions()
		{
			Items.Add(new CategoryElement(Environment, this, Data.Transaction.ComponentCategory, "Transactions", "Transactions", "fal fa-database"));
		}
	}
}
