using System.Collections.Generic;
using System.Linq;
using TomPIT.Analysis;
using TomPIT.Application.Events;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Dom;

namespace TomPIT.Application.Items
{
	internal class EventItems : ItemsBase
	{
		protected override void OnQueryDescriptors(IDomElement element, List<IItemDescriptor> items)
		{
			var s = element.Environment.Context.MicroService();
			var server = element.Environment.Context.Connection();

			var ds = server.GetService<IComponentService>().QueryComponents(s, DistributedEvent.ComponentCategory).OrderBy(f => f.Name);

			items.Add(Empty(SR.DevSelect, string.Empty));

			foreach (var i in ds)
				items.Add(new ItemDescriptor(i.Name, i.Name));

			var refs = server.GetService<IDiscoveryService>().References(element.Environment.Context.MicroService());

			var external = new List<ItemDescriptor>();

			foreach (var i in refs.MicroServices)
			{
				var ms = server.GetService<IMicroServiceService>().Select(i.MicroService);

				if (ms == null)
					continue;

				ds = server.GetService<IComponentService>().QueryComponents(server.ResolveMicroServiceToken(i.MicroService), DistributedEvent.ComponentCategory).OrderBy(f => f.Name);

				foreach (var j in ds)
				{
					var key = string.Format("{0}/{1}", ms.Name, j.Name);

					external.Add(new ItemDescriptor(key, key));
				}
			}

			if (external.Count > 0)
				items.AddRange(external.OrderBy(f => f.Text));
		}
	}
}
