﻿using TomPIT.Annotations.Design;
using TomPIT.Collections;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.Distributed;
using TomPIT.MicroServices.Design;

namespace TomPIT.MicroServices.Distributed
{
	public class DistributedEvents : SourceCodeConfiguration, IDistributedEventsConfiguration
	{
		private ListItems<IDistributedEvent> _events = null;

		[Items(DesignUtils.DistributedEventItems)]
		public ListItems<IDistributedEvent> Events
		{
			get
			{
				if (_events == null)
					_events = new ListItems<IDistributedEvent> { Parent = this };

				return _events;
			}
		}
	}
}