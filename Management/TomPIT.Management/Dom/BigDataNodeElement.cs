using TomPIT.BigData;
using TomPIT.Ide.Dom;
using TomPIT.Ide.Dom.ComponentModel;
using TomPIT.Management.BigData;

namespace TomPIT.Management.Dom
{
	internal class BigDataNodeElement : TransactionElement
	{
		public BigDataNodeElement(IDomElement parent, INode node) : base(parent)
		{
			Node = node;
			Title = Node.Name;
			Id = Node.Token.ToString();
		}

		public INode Node { get; }
		public override object Component => Node;
		public override bool HasChildren => false;

		public override bool Commit(object component, string property, string attribute)
		{
			Environment.Context.Tenant.GetService<IBigDataManagementService>().UpdateNode(Node.Token, Node.Name,
				Node.ConnectionString, Node.AdminConnectionString, Node.Status, Node.Size);

			return true;
		}
	}
}
