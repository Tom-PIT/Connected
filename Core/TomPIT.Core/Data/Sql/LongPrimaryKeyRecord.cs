using System;

namespace TomPIT.Data.Sql
{
	[Serializable]
	public class LongPrimaryKeyRecord : DatabaseRecord, ILongPrimaryKeyRecord
	{
		private long _id = 0;

		public virtual long Id { get { return _id; } set { _id = value; } }

		public LongPrimaryKeyRecord() { }

		internal LongPrimaryKeyRecord(long id)
		{
			_id = id;
		}

		protected override void OnCreate()
		{
			if (IsDefined("id"))
				Id = GetLong("id");
		}
	}
}
