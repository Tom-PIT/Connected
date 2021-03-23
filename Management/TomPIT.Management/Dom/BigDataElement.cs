using TomPIT.Design.Ide;
using TomPIT.Ide.Dom;

namespace TomPIT.Management.Dom
{
	public class BigDataElement : DomElement
	{
		public const string DomId = "BigData";

		public BigDataElement(IEnvironment environment) : base(environment, null)
		{
			Id = DomId;
			Glyph = "fal fa-database";
			Title = "Big data";

			((Behavior)Behavior).AutoExpand = false;
		}

		public override bool HasChildren { get { return true; } }

		public override void LoadChildren()
		{
			Items.Add(new BigDataNodesElement(Environment, this));
			Items.Add(new BigDataPartitionsElement(this));
			Items.Add(new BigDataTransactionsElement(this));
		}

		public override void LoadChildren(string id)
		{
			if (string.Compare(id, BigDataNodesElement.FolderId, true) == 0)
				Items.Add(new BigDataNodesElement(Environment, this));
			else if (string.Compare(id, BigDataPartitionsElement.NodeId, true) == 0)
				Items.Add(new BigDataPartitionsElement(this));
			else if (string.Compare(id, BigDataTransactionsElement.NodeId, true) == 0)
				Items.Add(new BigDataTransactionsElement(this));
		}
	}
}
