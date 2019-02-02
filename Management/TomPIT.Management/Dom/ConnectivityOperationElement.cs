using System.Reflection;
using TomPIT.ComponentModel.Apis;

namespace TomPIT.Dom
{
	internal class ConnectivityOperationElement : TransactionElement
	{
		private IApiOperation _operation = null;

		public ConnectivityOperationElement(IDomElement parent, IApiOperation operation) : base(parent)
		{
			_operation = operation;
			Title = operation.Name;
			Id = operation.Id.ToString();
		}

		public override object Component => _operation;
		public override PropertyInfo Property => Configuration.GetType().GetProperty(nameof(Configuration.Protocols));
		private IApiOperation Configuration { get { return Component as IApiOperation; } }
	}
}
