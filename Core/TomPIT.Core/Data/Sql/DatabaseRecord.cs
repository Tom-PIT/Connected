using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Data.SqlClient;

namespace TomPIT.Data.Sql
{
	public abstract class DatabaseRecord
	{
		[NonSerialized]
		private Dictionary<string, bool> _mappings = null;
		[NonSerialized]
		private SqlDataReader _rdr = null;

		public void Create(SqlDataReader rdr)
		{
			_rdr = rdr;

			OnCreate();

			_rdr = null;
			_mappings = null;
		}

		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		protected internal SqlDataReader Reader { get { return _rdr; } }

		protected virtual void OnCreate()
		{

		}

		protected T GetValue<T>(string fieldName, T defaultValue) { return GetReaderValue<T>(Reader, fieldName, defaultValue); }
		protected int GetInt(string fieldName) { return GetReaderValue(Reader, fieldName, 0); }
		protected short GetShort(string fieldName) { return GetReaderValue<short>(Reader, fieldName, 0); }
		protected long GetLong(string fieldName) { return GetReaderValue(Reader, fieldName, 0L); }
		protected bool GetBool(string fieldName) { return GetReaderValue(Reader, fieldName, false); }
		protected bool GetBool(string fieldName, bool defaultValue) { return GetReaderValue(Reader, fieldName, defaultValue); }
		protected Guid GetGuid(string fieldName) { return GetReaderValue(Reader, fieldName, Guid.Empty); }
		protected DateTime GetDate(string fieldName) { return GetReaderValue(Reader, fieldName, DateTime.MinValue); }
		protected TimeSpan GetTimeSpan(string fieldName) { return GetReaderValue(Reader, fieldName, TimeSpan.Zero); }
		protected decimal GetDecimal(string fieldName) { return GetReaderValue(Reader, fieldName, 0m); }
		protected double GetDouble(string fieldName) { return GetReaderValue(Reader, fieldName, 0d); }
		protected string GetString(string fieldName) { return GetReaderValue(Reader, fieldName, string.Empty); }
		protected byte GetByte(string fieldName) { return GetReaderValue<byte>(Reader, fieldName, 0); }

		public bool IsDefined(string fieldName)
		{
			if (Mappings.ContainsKey(fieldName))
				return Mappings[fieldName];

			for (int i = 0; i < Reader.FieldCount; i++)
			{
				if (string.Compare(Reader.GetName(i), fieldName, false) == 0)
				{
					Mappings[fieldName] = true;
					return true;
				}
			}

			return false;
		}

		private Dictionary<string, bool> Mappings
		{
			get
			{
				if (_mappings == null)
					_mappings = new Dictionary<string, bool>();

				return _mappings;
			}
		}

		public static T GetReaderValue<T>(SqlDataReader rdr, string fieldName, T defaultValue)
		{
			int ordinal = rdr.GetOrdinal(fieldName);

			if (rdr.IsDBNull(ordinal))
				return defaultValue;
			else
				return (T)rdr.GetValue(ordinal);
		}
	}
}