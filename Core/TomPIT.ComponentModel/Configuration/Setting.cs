using System;

namespace TomPIT.Configuration
{
	internal class Setting : ISetting
	{
		public string Name { get; set; }
		public string Value { get; set; }
		public bool Visible { get; set; }
		public DataType DataType { get; set; }
		public string Tags { get; set; }
		public Guid ResourceGroup { get; set; }
	}
}
