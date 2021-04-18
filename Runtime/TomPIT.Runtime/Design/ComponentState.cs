using System;
using TomPIT.ComponentModel;

namespace TomPIT.Design
{
	public abstract class ComponentState : IComponentState
	{
		public IComponent Component { get; set; }

		public Guid Element { get; set; }

		public DateTime TimeStamp { get; set; }
	}
}
