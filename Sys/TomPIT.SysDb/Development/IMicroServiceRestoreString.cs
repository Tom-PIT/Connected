using System;
using TomPIT.ComponentModel;
using TomPIT.Globalization;

namespace TomPIT.SysDb.Development
{
	public interface IMicroServiceRestoreString
	{
		ILanguage Language { get; }
		Guid Element { get; }
		string Property { get; }
		string Value { get; }
		IMicroService MicroService { get; }
	}
}
