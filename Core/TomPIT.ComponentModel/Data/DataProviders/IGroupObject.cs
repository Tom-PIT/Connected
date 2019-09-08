using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Data.DataProviders
{
	public interface IGroupObject
	{
		string Text { get; }
		string Value { get; }
		string Description { get; }
	}
}
