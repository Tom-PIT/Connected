using System;
using System.ComponentModel;

namespace TomPIT.Data.Sql
{
	[Serializable]
	public class PrimaryKeyRecord : DatabaseRecord, IPrimaryKeyRecord
	{
		private int _id = 0;

		[Browsable(false)]
		public virtual int Id { get { return _id; } set { _id = value; } }

		public PrimaryKeyRecord() { }

		internal PrimaryKeyRecord(int id)
		{
			_id = id;
		}

		protected override void OnCreate()
		{
			if (IsDefined("id"))
				Id = GetInt("id");
		}
	}
}
