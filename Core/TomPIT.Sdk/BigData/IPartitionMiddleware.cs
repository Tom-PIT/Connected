using System.Collections.Generic;

namespace TomPIT.BigData
{
	public enum AggregateMode
	{
		None = 1,
		Sum = 2
	}

	public enum TimestampPrecision
	{
		Raw = 0,
		Second = 1,
		Minute = 2,
		Hour = 3,
		Day = 4,
		Week = 5,
		Month = 6,
		Year = 7,
	}
	public interface IPartitionMiddleware<T> : IPartitionComponent
	{
		List<T> Invoke(List<T> items);

		void Invoked(List<T> modifiedItems);
	}
}
