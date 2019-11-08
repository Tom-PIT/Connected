using System;

namespace TomPIT.ComponentModel
{
	public interface IComponentState
	{
		IComponent Component { get; }
		Guid Element { get; }
		DateTime TimeStamp { get; }
	}
}
