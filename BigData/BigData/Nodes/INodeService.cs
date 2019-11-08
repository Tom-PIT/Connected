using System;

namespace TomPIT.BigData.Nodes
{
	internal interface INodeService
	{
		INode SelectSmallest();
		INode Select(Guid token);

		void NotifyChanged(Guid token);
		void NotifyRemoved(Guid token);
	}
}
