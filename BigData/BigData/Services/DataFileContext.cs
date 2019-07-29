using System;
using System.Data;

namespace TomPIT.BigData.Services
{
	public enum DataFileContextMode
	{
		Update = 1,
		Full = 2
	}
	internal class DataFileContext
	{
		public IPartitionFile File { get; set; }
		public DataTable Data { get; set; }
		public Guid Lock { get; set; }

		public DataFileContextMode Mode { get; set; } = DataFileContextMode.Update;

		public DataTable TearOff()
		{
			var dt = Data.Clone();
			var limit = TransactionParser.FileSize;

			for (var i = Data.Rows.Count - 1; i >= limit; i--)
			{
				dt.Rows.Add(Data.Rows[i].ItemArray);
				Data.Rows.RemoveAt(i);
			}

			return dt;
		}
	}
}