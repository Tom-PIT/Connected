using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.BigData;
using TomPIT.Dom;
using TomPIT.Management.BigData;

namespace TomPIT.Management.Dom
{
	internal class BigDataNodeElement : TransactionElement
	{
		public BigDataNodeElement(IDomElement parent, INode node) : base(parent)
		{
			Node = node;
			Title = Node.Name;
			Id = Node.Token.AsString();
		}

		public INode Node { get; }
		public override object Component => Node;
		public override bool HasChildren => false;

		public override bool Commit(object component, string property, string attribute)
		{
			Connection.GetService<IBigDataManagementService>().UpdateNode(Node.Token, Node.Name,
				Node.ConnectionString, Node.AdminConnectionString, Node.Status, Node.Size);

			return true;
		}
	}
}
