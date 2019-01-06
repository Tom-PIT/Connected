using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Design
{
	public abstract class MicroServiceTemplate : IMicroServiceTemplate
	{
		public abstract Guid Token { get; }
		public abstract string Name { get; }

		public virtual List<IItemDescriptor> QueryDescriptors(IDomElement parent, string category)
		{
			return new List<IItemDescriptor>();
		}

		public virtual List<IDomElement> QueryDomRoot(IEnvironment environment)
		{
			return new List<IDomElement>();
		}

		public virtual List<IDomElement> QuerySecurityRoot(IDomElement parent)
		{
			return new List<IDomElement>();
		}

		protected IComponent CreateReferences(IEnvironment environment)
		{
			var ms = environment.Context.MicroService();
			var cs = environment.Context.Connection().GetService<IComponentService>();
			var cds = environment.Context.Connection().GetService<IComponentDevelopmentService>();

			var items = cs.QueryComponents(ms, "Reference");

			if (items != null && items.Count > 0)
				return items[0];

			var id = cds.Insert(null, ms, Guid.Empty, "Reference", "References", typeof(References).TypeName());
			var config = new References
			{
				Component = id
			};

			cds.Update(config);

			return cs.SelectComponent(id);
		}
	}
}
