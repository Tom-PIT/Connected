namespace TomPIT.ComponentModel.Resources
{
	public interface IStringResource : IConfigurationElement
	{
		string Key { get; }
		string DefaultValue { get; }
		bool IsLocalizable { get; }

		ListItems<IStringTranslation> Translations { get; }
	}
}
