using System.Collections.Generic;
using TomPIT.IoT;

namespace TomPIT.MicroServices.IoT.Models
{
	public interface IDataForwardingProvider
	{
		List<IIoTFieldState> ForwardState { get; }
	}
}
