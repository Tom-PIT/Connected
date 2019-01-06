using TomPIT.Annotations;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Data;
using TomPIT.ComponentModel.Events;

namespace TomPIT.Application.Data
{
	[Create("Transaction")]
	[DomDesigner("TomPIT.Application.Design.TransactionDesigner, TomPIT.Templates.Application")]
	public class Transaction : DataElement, ITransaction
	{
		public const string ComponentCategory = "Transaction";

		[Items("TomPIT.Application.Items.TransactionParameterCollection, TomPIT.Templates.Application")]
		public override ListItems<IDataParameter> Parameters => base.Parameters;

		[EventArguments(typeof(TransactionExecutedArguments))]
		public override IServerEvent Executed => base.Executed;
		[EventArguments(typeof(TransactionExecutingArguments))]
		public override IServerEvent Executing => base.Executing;
		[EventArguments(typeof(TransactionPreparingArguments))]
		public override IServerEvent Preparing => base.Preparing;
	}
}
