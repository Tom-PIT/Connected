﻿using System;
using TomPIT.Environment;

namespace TomPIT.Diagnostics
{
	internal class Metric : IMetric
	{
		public Guid Session { get; set; }
		public DateTime Start { get; set; }
		public DateTime End { get; set; }
		public SessionResult Result { get; set; } = SessionResult.Fail;
		public InstanceType Instance { get; set; }
		public string IP { get; set; }
		public Guid Component { get; set; }
		public Guid Element { get; set; }
		public Guid Parent { get; set; }
		public string Request { get; set; }
		public string Response { get; set; }
		public long ConsumptionIn { get; set; }
		public long ConsumptionOut { get; set; }
	}
}