using TomPIT.Globalization;
using TomPIT.Ide.Dom;
using TomPIT.Ide.Dom.ComponentModel;
using TomPIT.Management.Globalization;

namespace TomPIT.Management.Dom
{
	internal class LanguageElement : TransactionElement
	{
		public LanguageElement(IDomElement parent, ILanguage language) : base(parent)
		{
			Language = language;
			Title = Language.Name;
			Id = Language.Token.ToString();
		}

		public ILanguage Language { get; }
		public override object Component => Language;
		public override bool HasChildren => false;

		public override bool Commit(object component, string property, string attribute)
		{
			Environment.Context.Tenant.GetService<IGlobalizationManagementService>().UpdateLanguage(Language.Token, Language.Name,
				Language.Lcid, Language.Status, Language.Mappings);

			return true;
		}
	}
}
