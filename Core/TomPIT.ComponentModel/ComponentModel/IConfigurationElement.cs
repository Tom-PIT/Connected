namespace TomPIT.ComponentModel
{
	public interface IConfigurationElement : IElement
	{
		IElementValidation Validation { get; }
	}
}
