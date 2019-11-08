using System;
using System.ComponentModel;
using TomPIT.Annotations.Design;

namespace TomPIT.ComponentModel
{
	public abstract class ComponentConfiguration : IConfiguration, IElement
	{
		[Browsable(false)]
		[KeyProperty]
		public Guid Component { get; set; }

		Guid IElement.Id => Component;

		IElement IElement.Parent => null;

		public virtual void ComponentCreated()
		{

		}

		void IElement.Reset()
		{

		}
	}
}
