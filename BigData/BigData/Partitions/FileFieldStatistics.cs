
using Amt.Api.Data;

namespace Amt.DataHub.Partitions
{
	internal class FileFieldStatistics<T> : LongPrimaryKeyRecord
	{
		public T Min { get; set; }
		public T Max { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Min = GetValue<T>("minv", default(T));
			Max = GetValue<T>("maxv", default(T));
		}
	}
}