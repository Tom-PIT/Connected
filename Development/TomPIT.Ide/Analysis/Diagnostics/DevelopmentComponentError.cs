using System;
using TomPIT.Development;

namespace TomPIT.Ide.Analysis.Diagnostics
{
	internal class DevelopmentComponentError : DevelopmentError, IDevelopmentComponentError
	{
		public Guid MicroService { get; set; }
		public string ComponentName { get; set; }
		public string ComponentCategory { get; set; }
	}
}