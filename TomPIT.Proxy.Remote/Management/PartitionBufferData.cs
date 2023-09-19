using TomPIT.BigData;

namespace TomPIT.Proxy.Remote.Management
{
	internal class PartitionBufferData : IPartitionBufferData
	{
		public long Id { get; set; }

		public byte[] Data { get; set; }
	}
}
