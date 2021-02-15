using System;

namespace TomPIT.ComponentModel
{
	internal class ConfigurationSerializationState
	{
		public byte[] State { get; set; }
		public Type Type { get; set; }
		//public IConfiguration Instance {get; set;}
	}
}
