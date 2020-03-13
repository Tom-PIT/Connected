using TomPIT.ComponentModel.BigData;

namespace TomPIT.BigData
{
	public enum AggregateMode
	{
		None = 1,
		Sum = 2
	}

	public interface IBigDataService
	{
		void Update<T>(IPartitionConfiguration partition, T items);
	}
}
