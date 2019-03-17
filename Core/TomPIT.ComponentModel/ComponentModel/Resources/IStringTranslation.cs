namespace TomPIT.ComponentModel.Resources
{
	public interface IStringTranslation : IConfigurationElement
	{
		int Lcid { get; }
		string Value { get; }
	}
}
