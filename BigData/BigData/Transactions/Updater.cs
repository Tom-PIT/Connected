using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using TomPIT.BigData.Data;
using TomPIT.BigData.Partitions;
using TomPIT.BigData.Persistence;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
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
				throw new RuntimeException(SR.ErrBigDataPartitionNotActive);

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
				return;

			var content = MiddlewareDescriptor.Current.Tenant.GetService<IStorageService>().Download(blobs[0].Token);

			if (content == null || content.Content == null)
				return;

			Items = Serializer.Deserialize<JArray>(Encoding.UTF8.GetString(content.Content));

			if (Items == null || Items.Count == 0)
				return;

			CreateSchema();
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
	}
}