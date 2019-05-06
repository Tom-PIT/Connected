using System;
using System.Collections.Generic;
using System.Text;
using TomPIT.Development;

namespace TomPIT.Ide.Design
{
	public class DevelopmentComponentError : IDevelopmentComponentError
	{
		public Guid Element {get;set;}
		public DevelopmentSeverity Severity {get;set;}
		public string Message {get;set;}
	}
}
