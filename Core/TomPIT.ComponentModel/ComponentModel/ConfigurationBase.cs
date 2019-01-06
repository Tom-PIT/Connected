using System;
using System.ComponentModel;
using TomPIT.Annotations;

namespace TomPIT.ComponentModel
{
	public abstract class ConfigurationBase : IConfiguration, IElement
	{
		[Browsable(false)]
		[KeyProperty]
		public Guid Component { get; set; }

		Guid IElement.Id => Component;

		IElement IElement.Parent => null;

		public virtual void ComponentCreated(IComponent scope)
		{

		}

		void IElement.Reset()
		{

		}
	}
}
