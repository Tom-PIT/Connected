using System;
using System.Collections.Generic;
using System.Reflection;
using TomPIT.ComponentModel;
using TomPIT.Ide.ComponentModel;
using TomPIT.Security;

namespace TomPIT.Ide.Dom.ComponentModel
{
	public abstract class ElementPermissionElement : ReflectionElement, IPermissionElement
	{
		public ElementPermissionElement(ReflectorCreateArgs e) : base(e)
		{
		}

		public ElementPermissionElement(IDomElement parent, object instance) : base(parent, instance)
		{
		}

		public ElementPermissionElement(IDomElement parent, object instance, PropertyInfo property, int index) : base(parent, instance, property, index)
		{
		}

		public abstract List<string> Claims { get; }
		public abstract IPermissionDescriptor PermissionDescriptor { get; }

		public string PrimaryKey
		{
			get
			{
				if (ConfigurationElement == null)
					return null;

				return ConfigurationElement.Id.ToString();
			}
		}

		public virtual bool SupportsInherit => false;
		public virtual string PermissionComponent => null;
		public virtual Guid ResourceGroup => DomQuery.Closest<IMicroServiceScope>(this).MicroService.ResourceGroup;

		protected IElement ConfigurationElement { get { return Component as IElement; } }
	}
}
