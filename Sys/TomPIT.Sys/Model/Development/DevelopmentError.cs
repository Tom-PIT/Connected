﻿using System;
using TomPIT.Development;

namespace TomPIT.Sys.Model.Development
{
	internal class DevelopmentError : IDevelopmentError
	{
		public Guid Element { get; set; }
		public DevelopmentSeverity Severity { get; set; }
		public string Message { get; set; }
		public string Code { get; set; }
		public ErrorCategory Category { get; set; }
		public Guid Component { get; set; }
		public Guid Identifier { get; set; }
	}
}
