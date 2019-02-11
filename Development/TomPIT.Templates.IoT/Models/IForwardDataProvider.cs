using System.Collections.Generic;

namespace TomPIT.IoT.Models
{
	public interface IForwardDataProvider
	{
		List<IIoTFieldState> ForwardState { get; }
	}
}
