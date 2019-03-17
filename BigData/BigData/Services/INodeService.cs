using System;

namespace TomPIT.BigData.Services
{
	internal interface INodeService
	{
		INode SelectSmallest();
		INode Select(Guid token);

		void NotifyChanged(Guid token);
		void NotifyRemoved(Guid token);
	}
}
