namespace TomPIT.ComponentModel.Resources
{
	public interface IStringTable : IConfiguration
	{
		ListItems<IStringResource> Strings { get; }
	}
}
