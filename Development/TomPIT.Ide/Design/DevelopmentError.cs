using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Development;

namespace TomPIT.Ide.Design
{
	internal class DevelopmentError : IDevelopmentError
	{
		public Guid MicroService { get; set; }
		public Guid Component { get; set; }
		public Guid Element { get; set; }
		public DevelopmentSeverity Severity { get; set; }
		public string Message { get; set; }
		public string ComponentName { get; set; }
		public string ComponentCategory { get; set; }
	}
}