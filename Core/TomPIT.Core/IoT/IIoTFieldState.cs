using System;

namespace TomPIT.IoT
{
	public interface IIoTFieldState : IIoTFieldStateModifier
	{
		DateTime Modified { get; }
	}
}
