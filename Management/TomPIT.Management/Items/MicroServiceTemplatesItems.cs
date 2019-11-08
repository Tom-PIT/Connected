using System.Collections.Generic;
using System.Linq;
using TomPIT.Ide.Collections;
using TomPIT.Ide.ComponentModel;
using TomPIT.Ide.Dom;

namespace TomPIT.Management.Items
{
	internal class MicroServiceTemplatesItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			var ds = element.Environment.Context.Tenant.GetService<IMicroServiceTemplateService>().Query().Where(f => f.Kind == TemplateKind.Standalone);

			foreach (var i in ds)
				items.Add(new ItemDescriptor(i.Name, i.Token.ToString()));
		}
	}
}
