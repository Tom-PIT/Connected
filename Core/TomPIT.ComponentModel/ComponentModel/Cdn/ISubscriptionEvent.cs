using System;
using TomPIT.ComponentModel.Events;

namespace TomPIT.ComponentModel.Cdn
{
	public interface ISubscriptionEvent : IConfigurationElement, ISourceCode
	{
		string Name { get; }
		[Obsolete]
		IServerEvent Invoke { get; }
	}
}
