using System;

namespace TomPIT.MicroServices.IoT.Annotations
{
	public enum IoTDesignerVerbs
	{
		None = 0,
		Select = 1,
		Move = 2,
		Resize = 4,
		All = 7
	}
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

		public IoTDesignerVerbs Verbs { get; set; } = IoTDesignerVerbs.All;
	}
}
