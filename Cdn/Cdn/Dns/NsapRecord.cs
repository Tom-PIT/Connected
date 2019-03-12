using System;

namespace TomPIT.Cdn.Dns
{
	internal class NsapRecord : IRecordData
	{
		public NsapRecord(DataBuffer buffer, int length)
		{
			buffer.Position += length;

			throw new NotImplementedException("Experimental Record Type Unable to Implement");
		}
	}
}
