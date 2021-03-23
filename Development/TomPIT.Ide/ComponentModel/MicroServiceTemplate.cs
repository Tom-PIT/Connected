using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Routing;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Design.Ide;
using TomPIT.Design.Ide.Dom;
using TomPIT.Reflection;

namespace TomPIT.Ide.ComponentModel
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

		public virtual void Initialize(IApplicationBuilder app, IWebHostEnvironment env)
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
			var cs = environment.Context.Tenant.GetService<IComponentService>();
			var cds = environment.Context.Tenant.GetService<IDesignService>().Components;

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

		public virtual void RegisterRoutes(IEndpointRouteBuilder builder)
		{

		}

		public virtual List<IIdeResource> ProvideIdeResources()
		{
			return null;
		}
	}
}
