using TomPIT.Collections;

namespace TomPIT.ComponentModel.Resources
{
	public interface IStringResource : IElement
	{
		string Key { get; }
		string DefaultValue { get; }
		bool IsLocalizable { get; }

		ListItems<IStringTranslation> Translations { get; }

		void UpdateTranslation(int lcid, string value, bool auditChange);
	}
}
