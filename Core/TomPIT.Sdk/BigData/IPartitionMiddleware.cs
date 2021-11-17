using System.Collections.Generic;

namespace TomPIT.BigData
{
	public enum AggregateMode
	{
		None = 1,
		Sum = 2
	}

	public interface IPartitionMiddleware<T> : IPartitionComponent
	{
		List<T> Invoke(List<T> items);

		void Invoked(List<T> modifiedItems);
	}
}
