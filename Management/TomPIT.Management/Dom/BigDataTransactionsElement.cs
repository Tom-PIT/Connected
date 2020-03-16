using TomPIT.Ide.Dom;

namespace TomPIT.Management.Dom
{
	public class BigDataTransactionsElement : DomElement
	{
		public const string NodeId = "BigDataTransactions";
		public BigDataTransactionsElement(IDomElement parent) : base(parent)
		{
			Title = "Transactions";
			Id = NodeId;
		}
	}
}
