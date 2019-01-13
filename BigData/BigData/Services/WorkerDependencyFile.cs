using Amt.Api.Data;
using System;

namespace Amt.DataHub
{
	public class WorkerDependencyFile : LongPrimaryKeyRecord
	{
		public long DependencyId { get; private set; }
		public Guid FileId { get; private set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			DependencyId = GetLong("dependecy_id");
			FileId = GetGuid("file_id");
		}
	}
}