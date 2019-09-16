using System.Collections.Generic;
using TomPIT.Globalization;
using TomPIT.Ide.Collections;
using TomPIT.Ide.Dom;

namespace TomPIT.Management.Items
{
	internal class LanguagesCollection : ItemsBase
	{
		public const string Token = "language";

		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			items.Add(new ItemDescriptor("Language", Token, typeof(ILanguage)));
		}
	}
}
