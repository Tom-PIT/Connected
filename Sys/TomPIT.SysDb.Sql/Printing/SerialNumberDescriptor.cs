using System.Threading;
using TomPIT.Data.Sql;
using TomPIT.SysDb.Printing;

namespace TomPIT.SysDb.Sql.Printing
{
	internal class SerialNumberDescriptor : LongPrimaryKeyRecord, ISerialNumber
	{
		private long _serialNumber = 0;
		public string Category {get;set;}

		public long SerialNumber => _serialNumber;

		public void Increment()
		{
			Interlocked.Increment(ref _serialNumber);
		}

		protected override void OnCreate()
		{
			base.OnCreate();

			Category = GetString("category");
			_serialNumber = GetLong("serial_number");
		}
	}
}
