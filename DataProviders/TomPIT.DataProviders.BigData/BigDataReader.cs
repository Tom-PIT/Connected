using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Data.Common;
using System.Linq;
using TomPIT.Connectivity;
using TomPIT.Diagnostics;
using TomPIT.Environment;
using TomPIT.Exceptions;
using TomPIT.Middleware;
using TomPIT.Reflection;

namespace TomPIT.DataProviders.BigData
{
	public class BigDataReader : DbDataReader
	{
		private JArray _data = null;
		public BigDataReader(BigDataCommand command)
		{
			Command = command;
			GetData();
		}

		private int ReadIndex { get; set; } = -1;
		private void GetData()
		{
			if (string.IsNullOrWhiteSpace(Command.Connection.DataSource))
				throw new RuntimeException($"{SR.ErrNoServer} ({InstanceFeatures.BigData}, {InstanceVerbs.Post})");

			if (string.IsNullOrWhiteSpace(Command.CommandText))
				throw new RuntimeException(nameof(BigDataCommand), SR.ErrCommandTextNull, LogCategories.BigData);

			var tokens = Command.CommandText.Split('/');
			var u = $"{Command.Connection.DataSource}/query/{tokens[0]}/{tokens[1]}";

			var args = new JArray();

			foreach (BigDataParameter parameter in Command.Parameters)
			{
				args.Add(new JObject
				{
					{parameter.ParameterName, new JValue( parameter.Value )}
				});
			};

			HttpRequestArgs credentialArgs = null;

			if (MiddlewareDescriptor.Current?.Identity?.IsAuthenticated ?? false)
			{
				credentialArgs = new HttpRequestArgs().WithCurrentCredentials(MiddlewareDescriptor.Current.User.AuthenticationToken);
			}

			_data = MiddlewareDescriptor.Current.Tenant.Post<JArray>(u, args, credentialArgs);
		}
		private BigDataCommand Command { get; }
		private JObject Current => ReadIndex == -1 || ReadIndex > _data.Count ? null : _data[ReadIndex] as JObject;
		public override object this[int ordinal] => ((JValue)Current.Properties().ElementAt(ordinal).Value).Value;
		public override object this[string name] => ((JValue)Current.Property(name, StringComparison.OrdinalIgnoreCase).Value).Value;
		private JObject First => _data is null || _data.Count == 0 ? null : _data[0] as JObject;
		public override int Depth => 1;

		public override int FieldCount
		{
			get
			{
				if (_data.Count == 0)
					return 0;

				var record = _data[0] as JObject;

				return record.Count;
			}
		}

		public override bool HasRows => _data.Count > 0;

		public override bool IsClosed => true;

		public override int RecordsAffected => _data.Count;

		public override bool GetBoolean(int ordinal)
		{
			return Types.Convert<bool>(this[ordinal]);
		}

		public override byte GetByte(int ordinal)
		{
			return Types.Convert<byte>(this[ordinal]);
		}

		public override long GetBytes(int ordinal, long dataOffset, byte[] buffer, int bufferOffset, int length)
		{
			throw new NotImplementedException();
		}

		public override char GetChar(int ordinal)
		{
			return Types.Convert<char>(this[ordinal]);
		}

		public override long GetChars(int ordinal, long dataOffset, char[] buffer, int bufferOffset, int length)
		{
			throw new NotImplementedException();
		}

		public override string GetDataTypeName(int ordinal)
		{
			return this[ordinal].GetType().ToFriendlyName();
		}

		public override DateTime GetDateTime(int ordinal)
		{
			return Types.Convert<DateTime>(this[ordinal]);
		}

		public override decimal GetDecimal(int ordinal)
		{
			return Types.Convert<decimal>(this[ordinal]);
		}

		public override double GetDouble(int ordinal)
		{
			return Types.Convert<double>(this[ordinal]);
		}

		public override IEnumerator GetEnumerator()
		{
			return _data.GetEnumerator();
		}

		public override Type GetFieldType(int ordinal)
		{
			return this[ordinal].GetType();
		}

		public override float GetFloat(int ordinal)
		{
			return Types.Convert<float>(this[ordinal]);
		}

		public override Guid GetGuid(int ordinal)
		{
			return Types.Convert<Guid>(this[ordinal]);
		}

		public override short GetInt16(int ordinal)
		{
			return Types.Convert<short>(this[ordinal]);
		}

		public override int GetInt32(int ordinal)
		{
			return Types.Convert<int>(this[ordinal]);
		}

		public override long GetInt64(int ordinal)
		{
			return Types.Convert<long>(this[ordinal]);
		}

		public override string GetName(int ordinal)
		{
			return First?.Properties().ElementAt(ordinal).Name;
		}

		public override int GetOrdinal(string name)
		{
			if (First is null)
				return -1;

			var index = 0;

			foreach (JProperty property in First.Properties())
			{
				if (string.Compare(property.Name, name, true) == 0)
					return index;

				index++;
			}

			return -1;
		}

		public override string GetString(int ordinal)
		{
			return Types.Convert<string>(this[ordinal]);
		}

		public override object GetValue(int ordinal)
		{
			return this[ordinal];
		}

		public override int GetValues(object[] values)
		{
			throw new NotImplementedException();
		}

		public override bool IsDBNull(int ordinal)
		{
			return false;
		}

		public override bool NextResult()
		{
			return false;
		}

		public override bool Read()
		{
			ReadIndex++;

			if (ReadIndex >= _data.Count)
			{
				ReadIndex--;

				return false;
			}

			return true;
		}
	}
}
