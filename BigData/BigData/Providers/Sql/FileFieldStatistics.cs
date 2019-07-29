using TomPIT.Data.Sql;

namespace TomPIT.BigData.Providers.Sql
{
	internal class FileFieldStatistics<T> : LongPrimaryKeyRecord
	{
		public T Min { get; set; }
		public T Max { get; set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Min = GetValue<T>("minv", default);
			Max = GetValue<T>("maxv", default);
		}
	}
}