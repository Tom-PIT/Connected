using TomPIT.Collections;

namespace TomPIT.ComponentModel.Resources
{
	public interface IStringTableConfiguration : IConfiguration
	{
		ListItems<IStringResource> Strings { get; }
	}
}
