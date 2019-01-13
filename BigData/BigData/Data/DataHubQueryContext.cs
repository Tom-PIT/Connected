namespace Amt.DataHub.Data
{
	internal class DataHubQueryContext
	{
		private int _count = 0;
		private object _sync = new object();

		public const int RowLimit = 50000;
		public bool IsFull { get { return Count > RowLimit; } }

		public void IncrementCount(int count)
		{
			lock (_sync)
			{
				_count += count;
			}
		}

		public int Count { get { return _count; } }
	}
}
