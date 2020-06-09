using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace TomPIT.DataProviders.BigData
{
	internal class BigDataParameters : DbParameterCollection
	{
		private List<DbParameter> _items = null;

		private List<DbParameter> Items
		{
			get
			{
				if (_items == null)
					_items = new List<DbParameter>();

				return _items;
			}
		}
		public override int Count => Items.Count;

		public override object SyncRoot => Items;

		public override int Add(object value)
		{
			if (!(value is DbParameter dp))
				throw new ArgumentException("Expected DbParameter");

			Items.Add(dp);

			return Count - 1;
		}

		public override void AddRange(Array values)
		{
			foreach (var i in values)
				Add(i);
		}

		public override void Clear()
		{
			Items.Clear();
		}

		public override bool Contains(object value)
		{
			foreach (var parameter in Items)
			{
				if (parameter == value)
					return true;
			}

			return false;
		}

		public override bool Contains(string value)
		{
			return false;
		}

		public override void CopyTo(Array array, int index)
		{
			throw new NotSupportedException();
		}

		public override IEnumerator GetEnumerator()
		{
			return Items.GetEnumerator();
		}

		public override int IndexOf(object value)
		{
			if (!(value is DbParameter dp))
				throw new ArgumentException("Expected DbParameter");

			return Items.IndexOf(dp);
		}

		public override int IndexOf(string parameterName)
		{
			foreach (var parameter in Items)
			{
				if (string.Compare(parameter.ParameterName, parameterName, true) == 0)
					return Items.IndexOf(parameter);
			}

			return -1;
		}

		public override void Insert(int index, object value)
		{
			if (!(value is DbParameter dp))
				throw new ArgumentException("Expected DbParameter");

			Items.Insert(index, dp);
		}

		public override void Remove(object value)
		{
			if (!(value is DbParameter dp))
				throw new ArgumentException("Expected DbParameter");

			Items.Remove(dp);
		}

		public override void RemoveAt(int index)
		{
			Items.RemoveAt(index);
		}

		public override void RemoveAt(string parameterName)
		{
			var idx = IndexOf(parameterName);

			if (idx > -1)
				RemoveAt(idx);
		}

		protected override DbParameter GetParameter(int index)
		{
			return Items[index];
		}

		protected override DbParameter GetParameter(string parameterName)
		{
			return Items.FirstOrDefault(f => string.Compare(f.ParameterName, parameterName, true) == 0);
		}

		protected override void SetParameter(int index, DbParameter value)
		{
			Items[index] = value;
		}

		protected override void SetParameter(string parameterName, DbParameter value)
		{
			var idx = IndexOf(parameterName);

			if (idx > -1)
				SetParameter(idx, value);
		}
	}
}
