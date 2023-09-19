using System.Collections.Immutable;
using TomPIT.Configuration;

namespace TomPIT.Proxy
{
	public interface ISettingController
	{
		ImmutableList<ISetting> Query();
		ISetting Select(string name, string nameSpace, string type, string primaryKey);
	}
}
