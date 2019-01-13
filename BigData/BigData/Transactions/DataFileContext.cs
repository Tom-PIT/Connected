using Amt.DataHub.Partitions;
using Amt.Sys.Model.DataHub;
using System.Data;

namespace Amt.DataHub.Transactions
{
	public enum DataFileContextMode
	{
		Update = 1,
		Full = 2
	}
	internal class DataFileContext
	{
		public PartitionFile File { get; set; }
		public DataTable Data { get; set; }
		public bool Locked { get; set; }

		public DataFileContextMode Mode { get; set; } = DataFileContextMode.Update;

		public DataTable TearOff()
		{
			var dt = Data.Clone();
			var limit = PipelineTransaction.FileSize;

			int total = File.Count + Data.Rows.Count;
			int overflow = total - limit;

			for (int i = Data.Rows.Count - 1; i >= limit; i--)
			{
				dt.Rows.Add(Data.Rows[i].ItemArray);
				Data.Rows.RemoveAt(i);
			}

			return dt;
		}
	}
}