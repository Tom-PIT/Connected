using System;
using System.Collections.Generic;

namespace TomPIT.IoT
{
	public class IoTStateChangedArgs : EventArgs
	{
		public IoTStateChangedArgs(Guid hub, List<IIoTFieldStateModifier> state)
		{
			Hub = hub;
			State = state;
		}

		public Guid Hub { get; }
		public List<IIoTFieldStateModifier> State { get; }
	}
}
