using System;
using System.Collections.Generic;
using System.Text;

namespace TomPIT.Annotations
{
	[AttributeUsage(AttributeTargets.Class)]
	public class AsyncPathAttribute : Attribute
	{
		public AsyncPathAttribute(string path)
		{
			Path = path;
		}

		public string Path { get; }
	}
}
