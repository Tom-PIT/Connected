using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Items
{
	internal class MicroServiceTemplatesItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			var ds = element.Environment.Context.GetServerContext().GetService<IMicroServiceTemplateService>().Query();

			foreach (var i in ds)
				items.Add(new ItemDescriptor(i.Name, i.Token.ToString()));
		}
	}
}
