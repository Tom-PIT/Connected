using System.Collections.Generic;

namespace TomPIT.Data
{
	public enum ConnectionBehavior
	{
		Shared = 1,
		Isolated = 2
	}
	public interface IDataReader<T> : IDataCommand
	{
		List<T> Query();
		T Select();
	}
}
