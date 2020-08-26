namespace TomPIT.BigData.Transactions
{
	internal class PartitionBufferData : IPartitionBufferData
	{
		public long Id { get; set; }

		public byte[] Data { get; set; }
	}
}
