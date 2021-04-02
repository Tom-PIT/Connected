using System;

namespace TomPIT.Reflection
{
	public interface IManifestDiscoveryNotification
	{
		void Invalidate(Guid microService, Guid component, Guid script);
		void NotifyChanged(Guid microService, Guid component, Guid script);
	}
}
