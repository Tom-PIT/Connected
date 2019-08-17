using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Dom;
using TomPIT.Globalization;
using TomPIT.Management.Globalization;

namespace TomPIT.Management.Dom
{
	internal class LanguageElement : TransactionElement
	{
		public LanguageElement(IDomElement parent, ILanguage language) : base(parent)
		{
			Language = language;
			Title = Language.Name;
			Id = Language.Token.AsString();
		}

		public ILanguage Language { get; }
		public override object Component => Language;
		public override bool HasChildren => false;

		public override bool Commit(object component, string property, string attribute)
		{
			Connection.GetService<IGlobalizationManagementService>().UpdateLanguage(Language.Token, Language.Name,
				Language.Lcid, Language.Status, Language.Mappings);

			return true;
		}
	}
}
