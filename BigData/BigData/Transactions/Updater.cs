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
using TomPIT.Serialization;
using TomPIT.Storage;

namespace TomPIT.BigData.Transactions
{
	internal class Updater : IUpdateProvider
	{
		private IPartitionConfiguration _configuration = null;
		private IMicroService _microService = null;
		private IPartition _partition = null;
		private object _sync = new object();

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
					_partition = Instance.Tenant.GetService<IPartitionService>().Select(Configuration);

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
					_configuration = Instance.Tenant.GetService<IComponentService>().SelectConfiguration(Block.Partition) as IPartitionConfiguration;

				return _configuration;
			}
		}

		private IMicroService MicroService
		{
			get
			{
				if (_microService == null)
					_microService = Instance.Tenant.GetService<IMicroServiceService>().Select(((IConfiguration)Configuration).MicroService());

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

					if (merger.Locked != null && merger.Locked.Rows.Count > 0)
					{
						lock (_sync)
						{
							if (LockedItems == null)
								LockedItems = new JArray();

							foreach (DataRow row in merger.Locked.Rows)
							{
								var lockedItem = new JObject();

								foreach (DataColumn column in row.Table.Columns)
									lockedItem.Add(new JProperty(column.ColumnName, row[column]));

								if (!string.IsNullOrWhiteSpace(f.Key))
									lockedItem.Add(new JProperty(Schema.PartitionKeyField, f.Key));

								LockedItems.Add(lockedItem);
							}
						}
					}
				});
		}

		private void LoadData()
		{
			var blobs = Instance.Tenant.GetService<IStorageService>().Query(MicroService.Token, BlobTypes.BigDataTransactionBlock, MicroService.ResourceGroup, Block.Token.ToString());

			if (blobs.Count == 0)
				return;

			var content = Instance.Tenant.GetService<IStorageService>().Download(blobs[0].Token);

			if (content == null || content.Content == null)
				return;

			Items = Serializer.Deserialize<JArray>(Encoding.UTF8.GetString(content.Content));

			if (Items == null || Items.Count == 0)
				return;

			CreateSchema();
		}

		private void CreateSchema()
		{
			Data = new Dictionary<string, DataTable>(StringComparer.OrdinalIgnoreCase);
			Schema = Instance.Tenant.GetService<IPersistenceService>().SelectSchema(Configuration);

			var schema = new DataTable();

			foreach (var field in Schema.Fields)
				schema.Columns.Add(field.Name, field.Type);

			foreach (JObject item in Items)
			{
				var partitionKeyProperty = item.Property(Schema.PartitionKeyField, StringComparison.OrdinalIgnoreCase);
				var partitionKeyValue = partitionKeyProperty == null ? string.Empty : partitionKeyProperty.Value<string>();

				DataTable table = null;

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