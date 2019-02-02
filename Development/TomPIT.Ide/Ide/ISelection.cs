using System.Collections.Generic;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Ide
{
	public interface ISelection : IEnvironmentClient
	{
		IPropertyProvider Properties { get; }
		IEventProvider Events { get; }

		IDomElement Element { get; }
		string Path { get; }
		string Property { get; }
		string Id { get; }
		IDomDesigner Designer { get; }
		ITransactionHandler Transaction { get; }

		List<IItemDescriptor> AddItems { get; }

		void Reset();
	}
}
