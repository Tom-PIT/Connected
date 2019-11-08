using System.Collections.Generic;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Designers;
using TomPIT.Ide.Dom;

namespace TomPIT.Ide.Environment.Providers
{
	public interface ISelectionProvider : IEnvironmentObject
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
