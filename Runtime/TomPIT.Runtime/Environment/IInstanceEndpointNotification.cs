﻿namespace TomPIT.Environment
{
	public interface IInstanceEndpointNotification
	{
		void NotifyChanged(object sender, InstanceEndpointEventArgs e);
		void NotifyRemoved(object sender, InstanceEndpointEventArgs e);
	}
}
