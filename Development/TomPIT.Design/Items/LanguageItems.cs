using System;
using System.Collections.Generic;
using TomPIT.Dom;
using TomPIT.Globalization;

namespace TomPIT.Design.Items
{
	internal class LanguageItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			var ds = element.Environment.Context.Connection().GetService<ILanguageService>().Query();

			items.Add(new ItemDescriptor(SR.DevLiDefault, Guid.Empty.ToString()));

			foreach (var i in ds)
				items.Add(new ItemDescriptor(i.Name, i.Token.ToString()));
		}
	}
}
