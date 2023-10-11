using TomPIT.Annotations;
using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Distributed
{
	public class Queue : ComponentConfiguration, IQueueConfiguration
	{
		private ListItems<IQueueWorker> _ops = null;

		[PropertyCategory(PropertyCategoryAttribute.CategoryDesign)]
		public string Namespace { get; set; }

		[Items(DesignUtils.QueueWorkerItems)]
		public ListItems<IQueueWorker> Workers
		{
			get
			{
				if (_ops == null)
					_ops = new ListItems<IQueueWorker> { Parent = this };

				return _ops;
			}
		}
	}
}
