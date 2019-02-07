using System;

namespace TomPIT.IoT.Annotations
{
	[AttributeUsage(AttributeTargets.Class)]
	public class IoTElementAttribute : Attribute
	{
		public IoTElementAttribute(Type model, string view)
		{
			View = view;
			Model = model;
		}
		public string View { get; }
		public Type Model { get; }
	}
}
