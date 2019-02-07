using System;

namespace TomPIT.IoT.Annotations
{
	[AttributeUsage(AttributeTargets.Class)]
	public class IoTElementAttribute : Attribute
	{
		public IoTElementAttribute(Type model, string view, string designView)
		{
			View = view;
			Model = model;
			DesignView = designView;
		}

		public string View { get; }
		public string DesignView { get; }
		public Type Model { get; }
	}
}
