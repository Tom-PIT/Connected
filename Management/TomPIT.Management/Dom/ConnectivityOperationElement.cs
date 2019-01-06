using System.Reflection;
using TomPIT.ComponentModel;
using TomPIT.Ide;

namespace TomPIT.Dom
{
	internal class ConnectivityOperationElement : TransactionElement
	{
		private IApiOperation _operation = null;

		public ConnectivityOperationElement(IEnvironment environment, IDomElement parent, IApiOperation operation) : base(environment, parent)
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
