using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace TomPIT.IoT
{
	public class IoTStateChangedArgs : EventArgs
	{
		private List<IIoTFieldStateModifier> _state = null;

		public IoTStateChangedArgs() { }

		public IoTStateChangedArgs(Guid hub, List<IIoTFieldStateModifier> state)
		{
			Hub = hub;
			State = state;
		}

		public Guid Hub { get; set; }
		[JsonConverter(typeof(IoTStateConverter))]
		public List<IIoTFieldStateModifier> State
		{
			get
			{
				if (_state == null)
					_state = new List<IIoTFieldStateModifier>();

				return _state;
			}
			private set
			{
				_state = value;
			}
		}
	}
}
