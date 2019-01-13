using Amt.Api.Data;
using System;

namespace Amt.DataHub
{
	public class WorkerDependency : LongPrimaryKeyRecord
	{
		public Guid Worker { get; private set; }
		public Guid Partition { get; private set; }
		public DateTime NextVisible { get; private set; }

		protected override void OnCreate()
		{
			base.OnCreate();

			Worker = GetGuid("worker");
			Partition = GetGuid("partition");
			NextVisible = GetDate("next_visible");
		}
	}
}