using System;
using System.Collections.Immutable;

namespace TomPIT.ComponentModel
{
	public delegate void MicroServiceChangedHandler(object sender, MicroServiceEventArgs e);

	public interface IMicroServiceService
	{
		event MicroServiceChangedHandler MicroServiceChanged;
		event MicroServiceChangedHandler MicroServiceRemoved;
		event MicroServiceChangedHandler MicroServiceInstalled;

		IMicroService SelectByUrl(string url);
		IMicroService Select(Guid microService);
		IMicroService Select(string name);

		ImmutableList<IMicroService> Query();
		ImmutableList<IMicroService> Query(Guid user);

		string SelectString(Guid microService, Guid language, Guid element, string property);
		string SelectMeta(Guid microService);
	}
}