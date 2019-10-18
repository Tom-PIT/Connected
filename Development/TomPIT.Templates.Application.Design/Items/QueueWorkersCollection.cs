using System.Collections.Generic;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;
using TomPIT.MicroServices.Distributed;

namespace TomPIT.MicroServices.Design.Items
{
	internal class QueueWorkersCollection : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Worker", "Worker", typeof(QueueWorker)));
		}
	}
}
