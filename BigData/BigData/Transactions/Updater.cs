using Newtonsoft.Json.Linq;

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using TomPIT.BigData.Data;
using TomPIT.BigData.Partitions;
using TomPIT.BigData.Persistence;
using TomPIT.Compilation;
using TomPIT.ComponentModel;
using TomPIT.ComponentModel.BigData;
using TomPIT.Diagnostics;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;
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
		private IPartition Partition => _partition ??= Tenant.GetService<IPartitionService>().Select(Configuration);

		public int UpdateRowCount
		{
			get
			{
				if (Items is null || Items.Count == 0)
					return 0;

				if (LockedItems is null || LockedItems.Count == 0)
					return Items.Count;

				return Items.Count - LockedItems.Count;
			}
		}

		private IPartitionConfiguration Configuration => _configuration ??= Tenant.GetService<IComponentService>().SelectConfiguration(Block.Partition) as IPartitionConfiguration;

		private IMicroService MicroService => _microService ??= Tenant.GetService<IMicroServiceService>().Select(((IConfiguration)Configuration).MicroService());

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

			var modifiedEntries = new ConcurrentBag<JObject>();

			Parallel.ForEach(Data,
				  (f) =>
				  {
					  var merger = new Merger(this, f.Key, f.Value);

					  merger.Merge();

					  var items = CreateArray(f.Key, f.Value);

					  if (merger.Locked)
					  {
						  lock (_sync)
						  {
							  if (LockedItems is null)
								  LockedItems = new JArray();

							  if (items is not null)
							  {
								  foreach (JObject item in items)
									  LockedItems.Add(item);
							  }
						  }
					  }
					  else
					  {
						  var middlewareType = Tenant.GetService<ICompilerService>().ResolveType(MicroService.Token, Configuration, Configuration.ComponentName());
						  var argumentType = middlewareType.GetInterface(typeof(IPartitionMiddleware<>).FullName).GetGenericArguments()[0];

						  var payload = (IList)typeof(List<>).MakeGenericType(argumentType).CreateInstance();

						  if (items is not null)
							  PopulatePayload(payload, items);

						  OnInvoked(middlewareType, payload);
					  }
				  });
		}

		private void PopulatePayload(IList items, JArray sourceItems)
		{
			var outputType = items.GetType().GetGenericArguments()[0];

			foreach (var item in sourceItems)
			{
				var itemInstance = outputType.CreateInstance();

				Serializer.Populate(item, itemInstance);

				items.Add(itemInstance);
			}
		}

		private void OnInvoked(Type middlewareType, IList payload)
		{
			using var ctx = new MicroServiceContext(MicroService);

			var middleware = Tenant.GetService<ICompilerService>().CreateInstance<MiddlewareComponent>(ctx, middlewareType);

			var methods = middleware.GetType().GetMethods();
			MethodInfo target = null;

			foreach (MethodInfo method in methods)
			{
				if (string.Compare(method.Name, nameof(PartitionMiddleware<object>.Invoked), false) != 0)
					continue;

				var parameters = method.GetParameters();

				if (parameters.Length != 1)
					continue;

				if (parameters[0].ParameterType == payload.GetType())
				{
					target = method;
					break;
				}
			}

			target?.Invoke(middleware, new object[] { payload });
		}

		private void LoadData()
		{
			var blobs = Tenant.GetService<IStorageService>().Query(MicroService.Token, BlobTypes.BigDataTransactionBlock, MicroService.ResourceGroup, Block.Token.ToString());

			if (blobs.Count == 0)
			{
				Dump("no blobs");
				return;
			}

			var content = Tenant.GetService<IStorageService>().Download(blobs[0].Token);

			if (content == null || content.Content == null)
			{
				Dump("blob content null or empty");
				return;
			}

			Tenant.GetService<IStorageService>().Release(blobs[0].Token);

			Items = Serializer.Deserialize<JArray>(Encoding.UTF8.GetString(content.Content));

			if (Items == null || Items.Count == 0)
			{
				Dump("no items deserialized");
				return;
			}

			CreateSchema();

			foreach (var table in Data)
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

					if (property is not null)
					{
						var value = ((JValue)property.Value).Value;

						if (value == null || value == DBNull.Value)
							continue;

						var convertedValue = Types.Convert(value, field.Type);

						if (convertedValue is DateTime timeStampValue && string.Compare(field.Name, Merger.TimestampColumn, true) == 0)
						{
							if (Schema.SupportsTimezone && Block.Timezone != Guid.Empty)
							{
								var timezone = Tenant.GetService<ITimeZoneService>().Select(Block.Timezone);
								if (timezone is not null)
								{
									var systemTimezone = TimeZoneInfo.FindSystemTimeZoneById(timezone.Name);

									if (systemTimezone is not null)
										timeStampValue = timeStampValue.AddMinutes(systemTimezone.GetUtcOffset(timeStampValue).TotalMinutes);
								}
							}

							if (field is PartitionSchemaDateField dateField && dateField.Precision != TimestampPrecision.Raw)
							{
								switch (dateField.Precision)
								{
									case TimestampPrecision.Second:
										timeStampValue = new DateTime(timeStampValue.Year, timeStampValue.Month, timeStampValue.Day, timeStampValue.Hour, timeStampValue.Minute, timeStampValue.Second);
										break;
									case TimestampPrecision.Minute:
										timeStampValue = new DateTime(timeStampValue.Year, timeStampValue.Month, timeStampValue.Day, timeStampValue.Hour, timeStampValue.Minute, 0);
										break;
									case TimestampPrecision.Hour:
										timeStampValue = new DateTime(timeStampValue.Year, timeStampValue.Month, timeStampValue.Day, timeStampValue.Hour, 0, 0);
										break;
									case TimestampPrecision.Day:
										timeStampValue = new DateTime(timeStampValue.Year, timeStampValue.Month, timeStampValue.Day);
										break;
									case TimestampPrecision.Week:
										timeStampValue = timeStampValue.ToStartOfWeek(DayOfWeek.Monday);
										break;
									case TimestampPrecision.Month:
										timeStampValue = new DateTime(timeStampValue.Year, timeStampValue.Month, 1);
										break;
									case TimestampPrecision.Year:
										timeStampValue = new DateTime(timeStampValue.Year, 1, 1);
										break;
									default:
										break;
								}
							}

							if (Schema.SupportsTimezone && Block.Timezone != Guid.Empty)
							{
								var timezone = Tenant.GetService<ITimeZoneService>().Select(Block.Timezone);
								if (timezone is not null)
								{
									var systemTimezone = TimeZoneInfo.FindSystemTimeZoneById(timezone.Name);

									if (systemTimezone is not null)
										timeStampValue = new DateTime(timeStampValue.AddMinutes(-systemTimezone.GetUtcOffset(timeStampValue).TotalMinutes).Ticks, DateTimeKind.Utc);
								}
							}
							else
							{
								timeStampValue = new DateTime(timeStampValue.Ticks, DateTimeKind.Utc); ;
							}

							convertedValue = timeStampValue;
						}

						row[field.Name] = convertedValue;
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

			foreach (var item in Data)
			{
				var table = item.Value;
				var keys = new Dictionary<string, DataRow>();

				Console.WriteLine($"Tearing off: {table.Rows.Count}");

				var sanitized = table.Clone();

				for (var i = table.Rows.Count - 1; i >= 0; i--)
				{
					var row = table.Rows[i];
					var hash = ComputeHash(row, keyFields);

					if (keys.ContainsKey(hash))
					{
						if (aggregations)
							Aggregate(keys[hash], row);
					}
					else
						keys.Add(hash, sanitized.Rows.Add(row.ItemArray));
				}

				Data[item.Key] = sanitized;
			}
		}

		private static string ComputeHash(DataRow row, List<string> keyFields)
		{
			var sb = new StringBuilder();

			foreach (var field in keyFields)
			{
				var value = row[field];
				var text = value == null || value == DBNull.Value ? "_" : value.ToString();

				sb.Append($"{text}.");
			}

			return sb.ToString().ToLowerInvariant();
		}

		private void Aggregate(DataRow row, DataRow duplicate)
		{
			foreach (var field in Schema.Fields)
			{
				//TODO: we could probably optimize those conversions
				if (field is PartitionSchemaNumberField number && number.Aggregate == AggregateMode.Sum)
				{
					var value = Types.Convert(duplicate[field.Name], field.Type);
					var existingValue = Types.Convert(row[field.Name], field.Type);

					var calculated = Types.Convert<decimal>(value) + Types.Convert<decimal>(existingValue);

					row[field.Name] = Types.Convert(calculated, field.Type);
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
			if (!MiddlewareDescriptor.Current.Tenant.GetService<ILoggingService>().DumpEnabled)
				return string.Empty;

			using var ms = new MemoryStream();
			using var sw = new StreamWriter(ms, Encoding.UTF8);

			sw.Write(sw.NewLine);

			for (var i = 0; i < table.Columns.Count; i++)
			{
				sw.Write(table.Columns[i].ColumnName);

				if (i < table.Columns.Count - 1)
					sw.Write(",");
			}

			sw.Write(sw.NewLine);

			foreach (DataRow row in table.Rows)
			{
				for (var i = 0; i < table.Columns.Count; i++)
				{
					var value = string.Empty;

					if (row[i] == DBNull.Value || row[i] == null)
						value = "null";
					else
					{
						value = row[i].ToString();

						if (value.Contains(','))
							value = string.Format("\"{0}\"", value);
					}

					sw.Write(value);

					if (i < table.Columns.Count - 1)
						sw.Write(",");
				}

				sw.Write(sw.NewLine);
			}

			sw.Flush();
			ms.Seek(0, SeekOrigin.Begin);

			return Encoding.UTF8.GetString(ms.ToArray());
		}
	}
}