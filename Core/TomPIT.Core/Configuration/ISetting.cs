using System;

namespace TomPIT.Configuration
{
	public interface ISetting
	{
		string Name { get; }
		string Value { get; }
		bool Visible { get; }
		DataType DataType { get; }
		string Tags { get; }
		Guid ResourceGroup { get; }
	}
}
