namespace TomPIT.ComponentModel.Resources
{
	public interface IStringTranslation : IElement
	{
		int Lcid { get; }
		string Value { get; }
		bool Changed { get; }
	}
}
