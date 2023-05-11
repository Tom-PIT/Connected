using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TomPIT.Caching;
using TomPIT.Connectivity;

namespace TomPIT.IoT.Hubs
{
	internal class HubDataCache : ClientRepository<List<IIoTFieldState>, Guid>
	{
		public HubDataCache(ITenant tenant) : base(tenant, "hubdata")
		{
			Buffer = new();
		}

		private ConcurrentQueue<KeyValuePair<Guid, List<IIoTFieldStateModifier>>> Buffer { get; }

		public JObject Update(Guid hub, List<IIoTFieldStateModifier> fields)
		{
			var pending = new JObject
			{
				{"hub", hub }
			};

			var result = new JObject();
			var pendingFieldsArray = new JArray();

			pending.Add("fields", pendingFieldsArray);

			var schema = Tenant.GetService<IIoTService>().SelectState(hub);
			var resultProps = new Dictionary<string, JObject>(StringComparer.OrdinalIgnoreCase);

			lock (schema)
			{
				foreach (var i in fields)
				{
					SynchronizeField(schema, i);

					pendingFieldsArray.Add(new JObject
					{
						{"field", i.Field },
						{"value", i.Value },
						{"device", i.Device }
					});

					var deviceName = i.Device.Split('/')[^1];

					if (!resultProps.TryGetValue(i.Device, out JObject props))
					{
						props = new JObject();

						result.Add(deviceName, props);
						resultProps.Add(i.Device, props);
					}

					props.Add(i.Field.ToCamelCase(), new JValue(i.RawValue));
				}
			}

			Buffer.Enqueue(new KeyValuePair<Guid, List<IIoTFieldStateModifier>>(hub, fields));

			var hash = JsonConvert.SerializeObject(result);

			result.Add("$timestamp", DateTime.UtcNow);
			result.Add("$checkSum", Convert.ToBase64String(LZ4.LZ4Codec.Wrap(Hash(hash))));

			return result;
		}

		public byte[] Hash(string value)
		{
			if (string.IsNullOrEmpty(value))
				return new byte[0];

			using (var md = MD5.Create())
			{
				return Encoding.UTF8.GetBytes(GetHash(md, value));
			}
		}

		private string GetHash(MD5 hash, string value)
		{
			var data = hash.ComputeHash(Encoding.UTF8.GetBytes(value));
			var sb = new StringBuilder();

			for (var i = 0; i < data.Length; i++)
				sb.Append(data[i].ToString("x2"));

			return sb.ToString();
		}

		public void Flush()
		{
			while (!Buffer.IsEmpty)
			{
				if (!Buffer.TryDequeue(out KeyValuePair<Guid, List<IIoTFieldStateModifier>> i))
					break;

				Instance.SysProxy.IoT.UpdateState(i.Key, i.Value);
			}
		}

		private void SynchronizeField(List<IIoTFieldState> schema, IIoTFieldStateModifier modifier)
		{
			if (schema.FirstOrDefault(f => string.Compare(f.Device, modifier.Device, true) == 0 && string.Compare(f.Field, modifier.Field, true) == 0) is not IoTFieldState field)
			{
				schema.Add(new IoTFieldState
				{
					Field = modifier.Field,
					Modified = DateTime.UtcNow,
					Value = modifier.Value,
					Device = modifier.Device
				});
			}
			else
			{
				field.Modified = DateTime.UtcNow;
				field.Value = modifier.Value;
			}
		}
	}
}
