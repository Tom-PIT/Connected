using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using TomPIT.ComponentModel;
using TomPIT.Dom;
using TomPIT.Ide;

namespace TomPIT.Design
{
	public abstract class MicroServiceTemplate : IMicroServiceTemplate
	{
		public abstract Guid Token { get; }
		public abstract string Name { get; }

		public virtual TemplateKind Kind => TemplateKind.Standalone;

		public virtual List<string> GetApplicationParts()
		{
			return new List<string>();
		}

		public virtual void Initialize(IApplicationBuilder app, IHostingEnvironment env)
		{
		}

		public virtual List<IItemDescriptor> ProvideGlobalAddItems(IDomElement parent)
		{
			return new List<IItemDescriptor>();
		}

		public virtual List<IItemDescriptor> ProvideAddItems(IDomElement parent)
		{
			return new List<IItemDescriptor>();
		}

		public IComponent References(IEnvironment environment, Guid microService)
		{
			var cs = environment.Context.Connection().GetService<IComponentService>();
			var cds = environment.Context.Connection().GetService<IComponentDevelopmentService>();

			var items = cs.QueryComponents(microService, "Reference");

			if (items != null && items.Count > 0)
				return items[0];

			var id = cds.Insert(microService, Guid.Empty, "Reference", "References", typeof(References).TypeName());
			var config = new References
			{
				Component = id
			};

			cds.Update(config);

			return cs.SelectComponent(id);
		}
	}
}
