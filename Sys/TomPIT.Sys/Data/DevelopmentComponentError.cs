using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TomPIT.Development;

namespace TomPIT.Sys.Data
{
	internal class DevelopmentComponentError : IDevelopmentComponentError
	{
		public Guid Element { get; set; }
		public DevelopmentSeverity Severity { get; set; }
		public string Message { get; set; }
	}
}
