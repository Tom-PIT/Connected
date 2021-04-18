using System.Collections.Generic;
using TomPIT.Design.Ide.Designers;
using TomPIT.Design.Ide.Dom;
using TomPIT.Design.Ide.Events;
using TomPIT.Design.Ide.Properties;

namespace TomPIT.Design.Ide.Selection
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
