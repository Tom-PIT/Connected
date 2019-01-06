using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Design;

namespace TomPIT.Dom
{
	internal interface IComponentsElement : IDomElement
	{
		List<IItemDescriptor> Descriptors { get; }
		List<IComponent> Existing { get; }

		string Category { get; }
	}
}
