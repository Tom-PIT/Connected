using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TomPIT.BigData.Data;
using TomPIT.BigData.Partitions;
using TomPIT.BigData.Persistence;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Serialization;
using TomPIT.Storage;

namespace TomPIT.BigData.Transactions
{
	internal class Updater : IUpdateProvider
	{
		private IPartitionConfiguration _configuration = null;
		private IMicroService _microService = null;
		private IPartition _partition = null;
		private readonly object _sync = new object();

		public Updater(ITransactionBlock block)
		{
			Block = block;
		}

		public ITransactionBlock Block { get; }
		public JArray LockedItems { get; set; }
		private JArray Items { get; set; }
		public PartitionSchema Schema { get; set; }
		private Dictionary<string, DataTable> Data { get; set; }
		private IPartition Partition
		{
			get
			{
				if (_partition == null)
					_partition = MiddlewareDescriptor.Current.Tenant.GetService<IPartitionService>().Select(Configuration);

				return _partition;
			}
		}

		public int UpdateRowCount
		{
			get
			{
				if (Items == null || Items.Count == 0)
					return 0;

				if (LockedItems == null || LockedItems.Count == 0)
					return Items.Count;

				return Items.Count - LockedItems.Count;
			}
		}

		private IPartitionConfiguration Configuration
		{
			get
			{
				if (_configuration == null)
					_configuration = MiddlewareDescriptor.Current.Tenant.GetService<IComponentService>().SelectConfiguration(Block.Partition) as IPartitionConfiguration;

				return _configuration;
			}
		}

		private IMicroService MicroService
		{
			get
			{
				if (_microService == null)
					_microService = MiddlewareDescriptor.Current.Tenant.GetService<IMicroServiceService>().Select(((IConfiguration)Configuration).MicroService());

				return _microService;
			}
		}

		public void Execute()
		{
			if (Partition.Status != PartitionStatus.Active)
			{
				Dump(SR.ErrBigDataPartitionNotActive);
				throw new RuntimeException(SR.ErrBigDataPartitionNotActive);
			}

			LoadData();

			if (Data == null)
				return;

			Parallel.ForEach(Data,
				(f) =>
				{
					var merger = new Merger(this, f.Key, f.Value);

					merger.Merge();

					if (merger.Locked)
					{
						lock (_sync)
						{
							if (LockedItems == null)
								LockedItems = new JArray();

							foreach (var d in Data)
							{
								var items = CreateArray(d.Key, d.Value);

								if (items != null)
								{
									foreach (JObject item in items)
										LockedItems.Add(item);
								}
							}
						}
					}
				});
		}

		private void LoadData()
		{
			var blobs = MiddlewareDescriptor.Current.Tenant.GetService<IStorageService>().Query(MicroService.Token, BlobTypes.BigDataTransactionBlock, MicroService.ResourceGroup, Block.Token.ToString());

			if (blobs.Count == 0)
			{
				Dump("no blobs");
				return;
			}

			var content = MiddlewareDescriptor.Current.Tenant.GetService<IStorageService>().Download(blobs[0].Token);

			if (content == null || content.Content == null)
			{
				Dump("blob content null or empty");
				return;
			}

			Items = Serializer.Deserialize<JArray>(Encoding.UTF8.GetString(content.Content));

			if (Items == null || Items.Count == 0)
			{
				Dump("no items deserialized");
				return;
			}

			CreateSchema();

			foreach(var table in Data)
				Dump($"table: {table.Key}, {ToCsv(table.Value)}");

			TearOff();

			foreach (var table in Data)
				Dump($"tearoff table: {table.Key}, {ToCsv(table.Value)}");
		}

		private JArray CreateArray(string key, DataTable data)
		{
			var result = new JArray();

			foreach (DataRow row in data.Rows)
			{
				var record = new JObject();

				if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(Schema.PartitionKeyField))
					record.Add(new JProperty(Schema.PartitionKeyField, key));

				foreach (DataColumn column in data.Columns)
				{
					var value = row[column];

					if (value == null || value == DBNull.Value)
						continue;

					record.Add(new JProperty(column.ColumnName, value));
				}

				if (record.Count > 0)
					result.Add(record);
			}

			return result;
		}
		private void CreateSchema()
		{
			Data = new Dictionary<string, DataTable>(StringComparer.OrdinalIgnoreCase);
			Schema = MiddlewareDescriptor.Current.Tenant.GetService<IPersistenceService>().SelectSchema(Configuration);

			var schema = new DataTable();

			foreach (var field in Schema.Fields)
				schema.Columns.Add(field.Name, field.Type);

			foreach (JObject item in Items)
			{
				var partitionKeyProperty = item.Property(Schema.PartitionKeyField, StringComparison.OrdinalIgnoreCase);
				var partitionKeyValue = partitionKeyProperty == null ? string.Empty : Types.Convert<string>(partitionKeyProperty.Value);

				DataTable table;

				if (Data.ContainsKey(partitionKeyValue))
					table = Data[partitionKeyValue];
				else
				{
					table = schema.Clone();
					Data.Add(partitionKeyValue, table);
				}

				var row = table.NewRow();

				row[Merger.TimestampColumn] = DateTime.UtcNow;

				foreach (var field in Schema.Fields)
				{
					var property = item.Property(field.Name, StringComparison.OrdinalIgnoreCase);

					if (property != null)
					{
						var value = ((JValue)property.Value).Value;

						if (value == null || value == DBNull.Value)
							continue;

						row[field.Name] = Types.Convert(value, field.Type);
					}
				}

				table.Rows.Add(row);
			}
		}

		private void TearOff()
		{
			var keyFields = KeyFields();

			if (keyFields.Count == 0)
				return;

			var aggregations = HasAggregations;

			foreach (var table in Data.Values)
			{
				var removables = new List<DataRow>();
				
				Console.WriteLine($"Tearing off: {table.Rows.Count}");

				for(var i = 0; i < table.Rows.Count; i++)
				{
					var row = table.Rows[i];
					var duplicates = Duplicates(table, row, keyFields);

					if (duplicates == null)
						continue;

					if (HasAggregations)
						Aggregate(row, duplicates.Select(f => f.Row).ToList());

					removables.AddRange(duplicates.Select(f => f.Row));
				}

				foreach (var row in removables)
					table.Rows.Remove(row);
			}
		}

		private List<DataRowDuplicate> Duplicates(DataTable table, DataRow row, List<string> keyFields)
		{
			var result = new List<DataRowDuplicate>();

			for(var i = 0; i < table.Rows.Count; i++)
			{
				var dr = table.Rows[i];
				var match = true;

				foreach (var field in keyFields)
				{
					if (Comparer.Default.Compare(row[field], dr[field]) != 0)
					{
						match = false;
						break;
					}
				}

				if (match)
				{
					result.Add(new DataRowDuplicate
					{
						Index = i,
						Row = dr,
						Timestamp = Types.Convert<DateTime>(dr[Merger.TimestampColumn])
					});
				}
			}

			if (result.Count == 1)
				return null;

			result = result.OrderBy(f => f.Timestamp).ThenBy(f => f.Index).ToList();

			if (result[^1].Row != row)
				return null;

			result.RemoveAt(result.Count - 1);

			return result;
		}

		private void Aggregate(DataRow row, List<DataRow> duplicates)
		{
			foreach (DataRow dr in duplicates)
			{
				foreach (var field in Schema.Fields)
				{
					if (field is PartitionSchemaNumberField number && number.Aggregate == AggregateMode.Sum)
					{
						var value = Types.Convert(dr[field.Name], field.Type);
						var existingValue = Types.Convert(row[field.Name], field.Type);

						var calculated = Types.Convert<decimal>(value) + Types.Convert<decimal>(existingValue);

						row[field.Name] = Types.Convert(calculated, field.Type);
					}
				}
			}
		}

		private List<string> KeyFields()
		{
			var result = new List<string>();

			foreach (var field in Schema.Fields)
			{
				if (field.Key || string.Compare(Merger.TimestampColumn, field.Name, true) == 0)
					result.Add(field.Name);
			}

			return result;
		}

		private bool HasAggregations
		{
			get
			{
				foreach (var field in Schema.Fields)
				{
					if (field is PartitionSchemaNumberField number)
					{
						if (number.Aggregate != AggregateMode.None)
							return true;
					}
				}

				return false;
			}
		}

		private void Dump(string text)
		{
			MiddlewareDescriptor.Current.Tenant.GetService<ILoggingService>().Dump($"Updater, Partition:{Partition.Configuration}, Transaction: {Block.Transaction}, Block: {Block.Token}, {text}.");
		}

		internal static string ToCsv(DataTable table)
		{
			using var ms = new MemoryStream();
			using var sw = new StreamWriter(ms, Encoding.UTF8);

			sw.Write(sw.NewLine);

			for (var i = 0; i < table.Columns.Count; i++)
			{
				sw.Write(table.Columns[i]);

				if (i < table.Columns.Count - 1)
					sw.Write(",");
			}

			sw.Write(sw.NewLine);

			foreach (DataRow row in table.Rows)
			{
				for (var i = 0; i < table.Columns.Count; i++)
				{
					var value = string.Empty;

					if (row[i] == DBNull.Value || string.IsNullOrEmpty(row[i] as string))
						value = "null";
					else
					{
						value = row[i].ToString();

						if (value.Contains(','))
							value = string.Format("\"{0}\"", value);

						sw.Write(value);
					}
					
					if (i < table.Columns.Count - 1)
						sw.Write(",");
				}

				sw.Write(sw.NewLine);
			}

			sw.Close();

			ms.Seek(0, SeekOrigin.Begin);

			return Encoding.UTF8.GetString(ms.ToArray());
		}
	}
}