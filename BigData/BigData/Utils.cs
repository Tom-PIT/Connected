using Amt.DataHub.Data;
using Amt.DataHub.Partitions;
using Amt.DataHub.Transactions;
using Amt.Sdk.DataHub;
using Amt.Sdk.Exceptions;
using Amt.Sys.Model.DataHub;
using System;
using System.Data;

namespace Amt.DataHub
{
	internal class Utils
	{
		public static DataTable ValidateDataTable(Partition partition, PartitionConfiguration config, DataTable table)
		{
			if (table == null)
				table = DataHubQuery.CreateSchema(config);

			foreach (var i in config.Schema)
			{
				if (!table.Columns.Contains(i.Name))
					throw new ApiException(string.Format("Field '{0}' not specified.", i.Name));
			}

			bool recreate = false;
			bool hasTimestamp = true;

			if (!table.Columns.Contains(PartitionTransaction.TimestampColumn))
			{
				recreate = true;
				hasTimestamp = false;
			}
			else
			{
				DataColumn col = table.Columns[PartitionTransaction.TimestampColumn];

				if (col.DataType != typeof(DateTime))
					throw new ApiException("Timestamp column doesn not contain date time data");
			}

			if (!recreate)
			{
				for (int i = 0; i < config.Schema.Count; i++)
				{
					var schemaField = config.Schema[i];
					var column = table.Columns[i];

					if (string.Compare(schemaField.Name, column.ColumnName, true) != 0)
					{
						recreate = true;
						break;
					}
				}
			}

			if (!recreate)
				return table;

			var dataTable = DataHubQuery.CreateSchema(config);

			foreach (DataRow i in table.Rows)
			{
				var row = dataTable.NewRow();

				foreach (DataColumn j in dataTable.Columns)
				{
					if (string.Compare(j.ColumnName, PartitionTransaction.TimestampColumn, true) == 0)
					{
						if (hasTimestamp)
						{
							var dt = i[PartitionTransaction.TimestampColumn];

							if (dt == null || dt == DBNull.Value)
								row[PartitionTransaction.TimestampColumn] = GetSystemTimestamp(partition, config);
							else
								row[PartitionTransaction.TimestampColumn] = dt;
						}
						else
							row[PartitionTransaction.TimestampColumn] = GetSystemTimestamp(partition, config);
					}
					else
						row[j.ColumnName] = i[j.ColumnName];
				}

				dataTable.Rows.Add(row);
			}

			return dataTable;
		}

		private static DateTime GetSystemTimestamp(Partition partition, PartitionConfiguration config)
		{
			switch (config.PartitionTimestampBehavior)
			{
				case PartitionTimestampBehavior.Static:
					return partition.Created;
				case PartitionTimestampBehavior.Dynamic:
					return DateTime.UtcNow;
				default:
					throw new NotSupportedException();
			}
		}
	}
}