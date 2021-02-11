using System;
using TomPIT.ComponentModel;

namespace TomPIT.Sys.Model.Components
{
	internal class ComponentState : IComponentState
	{
		public IComponent Component { get; set; }

		public Guid Element { get; set; }

		public DateTime TimeStamp { get; set; }
	}
}
