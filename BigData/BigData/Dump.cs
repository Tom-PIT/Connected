using Amt.Core.Diagnostics;
using System;
using System.Data;
using System.Text;

namespace Amt.DataHub
{
#if DUMP
	public static class Dump
	{
		internal static void CheckDependency(int partitionId, string partitionKey, DataTable data, Guid taskPopReceipt)
		{
			if (string.Compare(partitionKey, "1") == 0 && partitionId == 2)
			{
				StringBuilder sb = new StringBuilder();

				foreach (DataRow dr in data.Rows)
				{
					var id = Convert.ToInt32(dr["device_measurement_id"]);

					if (id != 3)
						continue;

					foreach (DataColumn dc in data.Columns)
					{
						sb.AppendFormat("{0} : {1}", dc.ColumnName, dr[dc]);
						sb.AppendLine();
					}
				}

				if (sb.Length != 0)
				{
					sb.Insert(0, Environment.NewLine);
					sb.Insert(0, string.Format("Partition: {0}, run time: {1}, task: {3}{2}", partitionId, DateTime.UtcNow.ToString("HH:mm:ss.fff"), Environment.NewLine, taskPopReceipt));

					AmtShell.GetService<ILoggingService>().Write("Partition Transaction", "CheckDependecies", sb.ToString(), 0, System.Diagnostics.TraceLevel.Info);
				}
			}
		}

		internal static void FullMerge(int partitionId, string partitionKey, DataTable data)
		{
			if (string.Compare(partitionKey, "1") == 0 && (partitionId == 2))
			{
				StringBuilder sb = new StringBuilder();

				foreach (DataRow dr in data.Rows)
				{
					var id = Convert.ToInt32(dr["device_measurement_id"]);

					if (id < 8)
						continue;

					foreach (DataColumn dc in data.Columns)
					{
						sb.AppendFormat("{0} : {1}", dc.ColumnName, dr[dc]);
						sb.AppendLine();
					}
				}

				if (sb.Length != 0)
				{
					sb.Insert(0, Environment.NewLine);
					sb.Insert(0, string.Format("Partition: {0}, run time: {1}{2}", partitionId, DateTime.UtcNow.ToString("HH:mm:ss.fff"), Environment.NewLine));

					AmtShell.GetService<ILoggingService>().Write("Partition Transaction", "Full Merge", sb.ToString(), 0, System.Diagnostics.TraceLevel.Info);
				}
			}
		}

		internal static void PartialMerge(int partitionId, string partitionKey, DataTable data)
		{
			if (string.Compare(partitionKey, "1") == 0 && (partitionId == 2 || partitionId == 3))
			{
				StringBuilder sb = new StringBuilder();

				foreach (DataRow dr in data.Rows)
				{
					var id = Convert.ToInt32(dr["device_measurement_id"]);

					if (id != 3)
						continue;

					foreach (DataColumn dc in data.Columns)
					{
						sb.AppendFormat("{0} : {1}", dc.ColumnName, dr[dc]);
						sb.AppendLine();
					}

				}

				if (sb.Length != 0)
				{
					sb.Insert(0, Environment.NewLine);
					sb.Insert(0, string.Format("Partition: {0}, run time: {1}{2}", partitionId, DateTime.UtcNow.ToString("HH:mm:ss.fff"), Environment.NewLine));

					AmtShell.GetService<ILoggingService>().Write("Partition Transaction", "Partial Merge", sb.ToString(), 0, System.Diagnostics.TraceLevel.Info);
				}
			}

		}

		public static void RunDependency(long workerDependencyId, Guid partition, DataTable data)
		{
			if (partition == new Guid("10911f3e-7043-47b2-a104-99a15b4ce365"))
			{
				StringBuilder sb = new StringBuilder();

				foreach (DataRow dr in data.Rows)
				{
					var id = Convert.ToInt32(dr["device_measurement_id"]);

					if (id != 3)
						continue;

					foreach (DataColumn dc in data.Columns)
					{
						sb.AppendFormat("{0} : {1}", dc.ColumnName, dr[dc]);
						sb.AppendLine();
					}

				}

				if (sb.Length != 0)
				{
					sb.Insert(0, Environment.NewLine);
					sb.Insert(0, string.Format("Partition: {0}, run time: {1}{2}, dep. id: {3}{2}", partition.AsString(), DateTime.UtcNow.ToString("HH:mm:ss.fff"), Environment.NewLine, workerDependencyId));

					AmtShell.GetService<ILoggingService>().Write("Partition Transaction", "Run dependency", sb.ToString(), 0, System.Diagnostics.TraceLevel.Info);
				}
			}
		}
	}
#endif
}