using System;
using System.Collections.Generic;
using TomPIT.ComponentModel;
using TomPIT.Design;
using TomPIT.Ide;
using TomPIT.Security;

namespace TomPIT.Dom
{
	public abstract class ComponentPermissionElement : ComponentElement, IPermissionElement
	{
		public ComponentPermissionElement(IEnvironment environment, IDomElement parent, IComponent component) : base(environment, parent, component)
		{
		}

		public abstract List<string> Claims { get; }
		public abstract IPermissionDescriptor PermissionDescriptor { get; }

		public string PrimaryKey => Target.Token.ToString();

		public virtual bool SupportsInherit => false;
		public virtual string PermissionComponent => null;
		public virtual Guid ResourceGroup => DomQuery.Closest<IMicroServiceScope>(this).MicroService.ResourceGroup;
	}
}
