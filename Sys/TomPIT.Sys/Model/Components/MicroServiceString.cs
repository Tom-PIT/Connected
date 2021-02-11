using System;
using TomPIT.ComponentModel;
using TomPIT.Globalization;
using TomPIT.SysDb.Development;

namespace TomPIT.Sys.Model.Components
{
	public class MicroServiceString : IMicroServiceRestoreString
	{
		public IMicroService MicroService { get; set; }
		public Guid Element { get; set; }
		public string Property { get; set; }
		public string Value { get; set; }
		public ILanguage Language { get; set; }
	}
}
