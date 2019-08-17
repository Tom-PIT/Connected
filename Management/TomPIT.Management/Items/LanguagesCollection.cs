using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Design;
using TomPIT.Dom;
using TomPIT.Globalization;

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
