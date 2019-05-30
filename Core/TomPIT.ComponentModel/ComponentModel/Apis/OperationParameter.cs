using System;

namespace TomPIT.ComponentModel.Apis
{
	public class OperationParameter
	{
		public string Name { get; set; }
		public Type Type { get; set; }
		public bool IsRequired { get; set; }
	}
}
