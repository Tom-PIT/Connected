using System;
using TomPIT.Data.Sql;
using TomPIT.Diagnostics;
using TomPIT.Environment;

namespace TomPIT.SysDb.Sql.Diagnostics
{
	internal class Metric : LongPrimaryKeyRecord, IMetric
	{
		public Guid Session { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public SessionResult Result { get; set; }
		public InstanceType Instance { get; set; }
		public string IP { get; set; }
		public Guid Component { get; set; }
		public Guid Element { get; set; }
		public Guid Parent { get; set; }
		public string Request { get; set; }
		public string Response { get; set; }
		public long ConsumptionIn { get; set; }
		public long ConsumptionOut { get; set; }

		protected override void OnCreate()
		{
			Session = GetGuid("session");
			Start = GetDate("start");
			End = GetDate("end");
			Result = GetValue("result", SessionResult.Success);
			Instance = GetValue("instance", InstanceType.Unknown);
			IP = GetString("request_ip");
			Component = GetGuid("component");
			Element = GetGuid("element");
			Parent = GetGuid("parent");
			Request = GetString("request");
			Response = GetString("response");
			ConsumptionIn = GetLong("consumption_in");
			ConsumptionOut = GetLong("consumption_out");
		}
	}
}
